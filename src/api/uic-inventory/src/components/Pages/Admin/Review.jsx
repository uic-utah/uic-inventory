import { Fragment, useContext, useMemo, useRef, useState } from 'react';
import { useQuery, useQueryClient, useMutation } from 'react-query';
import ky from 'ky';
import clsx from 'clsx';
import { useTable } from 'react-table';
import throttle from 'lodash.throttle';
import { Code } from 'react-content-loader';
import { Dialog, Listbox, Transition } from '@headlessui/react';
import { CheckIcon, SelectorIcon } from '@heroicons/react/solid';

import FeatureLayer from '@arcgis/core/layers/FeatureLayer';
import { SelectedWellsSymbol } from '../../MapElements/MarkerSymbols';

import { AuthContext } from '../../../AuthProvider';
import { FormGrid, ResponsiveGridColumn } from '../../FormElements';
import { ConfirmationModal, Chrome, Flagged, useParams, onRequestError, toast, useHistory } from '../../PageElements';
import { ownershipTypes, wellTypes, contactTypes, valueToLabel } from '../../../data/lookups';
import { useOpenClosed, useWebMap, useSitePolygon, useInventoryWells } from '../../Hooks';

const dateFormatter = new Intl.DateTimeFormat('en-US', {
  month: 'numeric',
  day: 'numeric',
  year: 'numeric',
  hour12: true,
  hour: 'numeric',
  minute: 'numeric',
});

export default function Review() {
  const { authInfo } = useContext(AuthContext);
  const history = useHistory();
  const { inventoryId, siteId } = useParams();
  const queryClient = useQueryClient();
  const [isOpen, { open, close }] = useOpenClosed();

  const { mutate } = useMutation((json) => ky.delete('/api/inventory/reject', { json }), {
    onSettled: () => {
      queryClient.invalidateQueries('sites');
      queryClient.invalidateQueries(['inventory', inventoryId]);
      queryClient.invalidateQueries(['site-inventories', inventoryId]);
    },
    onSuccess: () => {
      toast.success('Inventory rejected successfully!');
      history.replace('/');
    },
    onError: (error) => onRequestError(error, 'We had some trouble rejecting this inventory.'),
  });

  const reject = () => {
    const input = {
      accountId: parseInt(authInfo.id),
      siteId: parseInt(siteId),
      inventoryId: parseInt(inventoryId),
    };

    mutate(input);
  };

  return (
    <>
      <ConfirmationModal isOpen={isOpen} onClose={close} onYes={reject}>
        <Dialog.Title className="text-lg font-medium leading-6 text-gray-900">
          Reject Submission Confirmation
        </Dialog.Title>
        <Dialog.Description className="mt-1">This inventory will be permanently deleted</Dialog.Description>

        <p className="mt-1 text-sm text-gray-500">
          Are you sure you want to reject this submission? This action cannot be undone..
        </p>
      </ConfirmationModal>
      <Chrome title="Inventory Review">
        <SiteAndInventoryDetails siteId={siteId} inventoryId={inventoryId} />
        <LocationDetails siteId={siteId} inventoryId={inventoryId} />
        <ContactDetails siteId={siteId} />
        <WellDetails siteId={siteId} inventoryId={inventoryId} />
        <Section>
          <button
            onClick={open}
            className="inline-flex justify-center rounded-md border border-transparent bg-gray-800 px-4 py-2 font-medium text-white shadow-sm hover:bg-red-600 focus:outline-none focus:ring-2 focus:ring-gray-800 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-gray-400 disabled:opacity-50 sm:col-span-6 md:col-span-2"
          >
            Reject
          </button>
          <button meta="primary" className="rounded border sm:col-span-6 md:col-span-2">
            Print
          </button>
          <button meta="default" className="sm:col-span-6 md:col-span-2">
            Approve
          </button>
        </Section>
      </Chrome>
    </>
  );
}

const Label = ({ children }) => <span className="block font-bold text-gray-700">{children}</span>;

const Value = ({ children, className }) => <span className={clsx('ml-2 block', className)}>{children}</span>;

const Section = ({ gray, children, title, height = 'max-h-96' }) => (
  <>
    <h1 className="mb-2 text-xl font-medium">{title}</h1>
    <div className={`mb-3 ml-1 overflow-scroll border shadow sm:rounded-md ${height}`}>
      <div
        className={clsx(
          {
            'bg-gray-50': gray,
            'bg-white': !gray,
          },
          'h-full px-4 py-5 sm:p-6'
        )}
      >
        <FormGrid>{children}</FormGrid>
      </div>
    </div>
  </>
);

const SiteAndInventoryDetails = ({ siteId, inventoryId }) => {
  const { authInfo } = useContext(AuthContext);

  const { status, data } = useQuery(
    ['inventory', inventoryId],
    () => ky.get(`/api/site/${siteId}/inventory/${inventoryId}`).json(),
    {
      enabled: siteId > 0,
      onError: (error) => onRequestError(error, 'We had some trouble finding this inventory.'),
    }
  );

  const queryClient = useQueryClient();

  const { mutate } = useMutation((json) => ky.put('/api/inventory', { json }), {
    onMutate: async (inventory) => {
      await queryClient.cancelQueries(['inventory', inventoryId]);
      const previousValue = queryClient.getQueryData(['inventory', inventoryId]);

      queryClient.setQueryData(['inventory', inventoryId], (old) => {
        const updated = {
          ...old,
          site: { ...old.site },
          wells: [...old.wells],
        };

        if (inventory.subClass) {
          updated.subClass = inventory.subClass;
        }

        if (inventory.edocs) {
          updated.edocs = inventory.edocs;
        }

        return updated;
      });

      return previousValue;
    },
    onSettled: () => {
      queryClient.invalidateQueries(['inventory', inventoryId]);
    },
    onSuccess: () => {
      toast.success('Inventory updated successfully!');
    },
    onError: (error) => onRequestError(error, 'We had some trouble updating this inventory.'),
  });

  const updateSubClass = (newSubClass) => {
    const input = {
      accountId: parseInt(authInfo.id),
      inventoryId: parseInt(inventoryId),
      siteId: parseInt(siteId),
      subClass: newSubClass.value,
    };

    mutate(input);
  };

  const updateEdocs = (edocs) => {
    const input = {
      accountId: parseInt(authInfo.id),
      inventoryId: parseInt(inventoryId),
      siteId: parseInt(siteId),
      edocs,
    };

    mutate(input);
  };

  if (status === 'loading') {
    return <Code />;
  }

  return (
    <>
      <Flagged reason={data?.flagged} siteId={siteId} inventoryId={inventoryId} />
      <Section title="Site Details">
        <ResponsiveGridColumn full={true} half={true} third={true}>
          <Label>Name</Label>
          <Value>{data?.site.name}</Value>
        </ResponsiveGridColumn>
        <ResponsiveGridColumn full={true} half={true} third={true}>
          <Label>Address</Label>
          <Value>{data?.site.address}</Value>
        </ResponsiveGridColumn>
        <ResponsiveGridColumn full={true} half={true} third={true}>
          <Label>Land Ownership</Label>
          <Value>{valueToLabel(ownershipTypes, data?.site.ownership)}</Value>
        </ResponsiveGridColumn>
        <ResponsiveGridColumn full={true} half={true} third={true}>
          <Label>NAICS</Label>
          <Value>{`${data?.site.naicsPrimary} - ${data?.site.naicsTitle}`}</Value>
        </ResponsiveGridColumn>
        <EditableText field="Edocs #" initialValue={data?.edocs} onMutate={updateEdocs} />
      </Section>
      <Section title="Inventory Details">
        <EditableList
          field="Inventory Class"
          items={wellTypes}
          initialValue={valueToLabel(wellTypes, data?.subClass)}
          onMutate={updateSubClass}
        />
        <ResponsiveGridColumn full={true} half={true}>
          <Label>Order Number</Label>
          <Value>{data?.orderNumber}</Value>
        </ResponsiveGridColumn>
        <ResponsiveGridColumn full={true} half={true}>
          <Label>Signed By</Label>
          <Value>{data?.signature}</Value>
        </ResponsiveGridColumn>
        <ResponsiveGridColumn full={true} half={true}>
          <Label>Signed On</Label>
          <Value>{data?.submittedOn && dateFormatter.format(Date.parse(data?.submittedOn))}</Value>
        </ResponsiveGridColumn>
      </Section>
    </>
  );
};

const EditableText = ({ field, initialValue, onMutate }) => {
  const [active, { toggle }] = useOpenClosed();
  const [value, setValue] = useState(initialValue ?? '');

  const handleChange = () => {
    if (active) {
      onMutate(value);
    }

    toggle();
  };

  return (
    <ResponsiveGridColumn full={true} half={true} third={true}>
      <Label>
        {field}
        <button
          onClick={handleChange}
          className="ml-1 rounded-lg border px-2 py-1 text-xs hover:bg-gray-800 hover:text-white"
        >
          {active ? 'save' : 'modify'}
        </button>
        {active && (
          <button
            onClick={toggle}
            className="ml-1 rounded-lg border px-2 py-1 text-xs hover:bg-red-800 hover:text-white"
          >
            cancel
          </button>
        )}
      </Label>
      {!active ? (
        <Value>{value === '' ? '-' : value}</Value>
      ) : (
        <input type="text" value={value} onChange={(event) => setValue(event.target.value)} />
      )}
    </ResponsiveGridColumn>
  );
};

const EditableList = ({ field, initialValue, onMutate, items }) => {
  const [active, { toggle }] = useOpenClosed();
  const [selected, setSelected] = useState(() => {
    const results = items.filter((type) => type.label === initialValue);
    return results.length === 1 ? results[0] : results[0];
  });

  const handleChange = () => {
    if (active) {
      onMutate(selected);
    }

    toggle();
  };

  return (
    <ResponsiveGridColumn full={true} half={true}>
      <Label>
        {field}
        <button
          onClick={handleChange}
          className="ml-1 rounded-lg border px-2 py-1 text-xs hover:bg-gray-800 hover:text-white"
        >
          {active ? 'save' : 'modify'}
        </button>
        {active && (
          <button
            onClick={toggle}
            className="ml-1 rounded-lg border px-2 py-1 text-xs hover:bg-red-800 hover:text-white"
          >
            cancel
          </button>
        )}
      </Label>
      {!active ? <Value>{initialValue}</Value> : <MyListbox selected={selected} setSelected={setSelected} />}
    </ResponsiveGridColumn>
  );
};

const MyListbox = ({ selected, setSelected }) => {
  return (
    <div className="w-72">
      <Listbox value={selected} onChange={setSelected}>
        <Listbox.Button className="relative w-full cursor-default rounded-lg bg-white py-2 pl-3 pr-10 text-left shadow-md focus:outline-none focus-visible:border-indigo-500 focus-visible:ring-2 focus-visible:ring-white focus-visible:ring-opacity-75 focus-visible:ring-offset-2 focus-visible:ring-offset-orange-300 sm:text-sm">
          <span className="block truncate">{selected.label}</span>
          <span className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2">
            <SelectorIcon className="h-5 w-5 text-gray-400" aria-hidden="true" />
          </span>
        </Listbox.Button>
        <Transition as={Fragment} leave="transition ease-in duration-100" leaveFrom="opacity-100" leaveTo="opacity-0">
          <Listbox.Options className="absolute z-10 mt-1 max-h-60 max-w-min overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
            {wellTypes.map((subclass) => (
              <Listbox.Option
                key={subclass.value}
                className={({ active }) =>
                  clsx('relative cursor-default select-none py-2 pl-10 pr-4', {
                    'bg-gray-700 text-white': active,
                  })
                }
                value={subclass}
              >
                {({ selected, active }) => (
                  <>
                    <span className={`${selected ? 'font-medium' : 'font-normal'} block truncate`}>
                      {subclass.label}
                    </span>
                    {selected ? (
                      <span
                        className={clsx('absolute inset-y-0 left-0 flex items-center pl-3', {
                          'text-white': active,
                        })}
                      >
                        <CheckIcon className="h-5 w-5" aria-hidden="true" />
                      </span>
                    ) : null}
                  </>
                )}
              </Listbox.Option>
            ))}
          </Listbox.Options>
        </Transition>
      </Listbox>
    </div>
  );
};

const ContactDetails = ({ siteId }) => {
  const { status, data } = useQuery(['contacts', siteId], () => ky.get(`/api/site/${siteId}/contacts`).json(), {
    enabled: siteId > 0,
    onError: (error) => onRequestError(error, 'We had some trouble finding the contacts.'),
  });

  if (status === 'loading') {
    return <Code />;
  }

  return (
    <Section gray={true} title="Site Contacts">
      {data?.contacts.map((contact) => (
        <Panel key={contact.id}>
          <ResponsiveGridColumn full={true} half={true}>
            <Value className="-mx-3 mb-3 border border-r-0 border-blue-500 bg-blue-200 px-2 py-1 text-center font-bold text-blue-700 shadow">
              {valueToLabel(contactTypes, contact.contactType)}
            </Value>
          </ResponsiveGridColumn>
          <ResponsiveGridColumn full={true} half={true}>
            <Label>label</Label>
            <Value>
              {contact.firstName} {contact.LastName}
            </Value>
            <Value>{contact.organization}</Value>
          </ResponsiveGridColumn>
          <ResponsiveGridColumn full={true} half={true}>
            <Label>Contact</Label>
            <Value>{contact.email}</Value>
            <Value>{contact.phoneNumber}</Value>
          </ResponsiveGridColumn>
          <ResponsiveGridColumn full={true} half={true}>
            <Label>Address</Label>
            <Address {...contact} />
          </ResponsiveGridColumn>
        </Panel>
      ))}
    </Section>
  );
};

const handleLink = (text, siteId, inventoryId) => {
  if (text?.startsWith('file::')) {
    return (
      <a
        meta="primary"
        href={text.replace('file::', `/api/site/${siteId}/inventory/${inventoryId}/well/`).replaceAll('_', '/')}
        target="_blank"
        rel="noopener noreferrer"
      >
        attachment
      </a>
    );
  }

  return text;
};

const WellDetails = ({ siteId, inventoryId }) => {
  const { status, data } = useQuery(
    ['inventory', inventoryId],
    () => ky.get(`/api/site/${siteId}/inventory/${inventoryId}`).json(),
    {
      enabled: siteId > 0,
      onError: (error) => onRequestError(error, 'We had some trouble finding this inventory.'),
    }
  );

  if (status === 'loading') {
    return <Code />;
  }

  return (
    <>
      <Section gray={true} title="Construction Details">
        {data?.wells.map((well) => (
          <Panel key={well.id}>
            <div
              title="Well count"
              className="absolute inset-1 inline-flex h-8 w-8 items-center justify-center rounded-full border border-gray-800 bg-white/90 text-xs font-bold text-gray-700"
            >
              {well.count}
            </div>
            <Value className="-mx-3 mb-3 border border-r-0 border-blue-500 bg-blue-200 px-2 py-1 text-center font-bold text-blue-700 shadow">
              {well.status}
            </Value>
            <Label>Construction</Label>
            <Value>{handleLink(well.constructionDetails, siteId, inventoryId)}</Value>
          </Panel>
        ))}
      </Section>
      <Section gray={true} title="Injectate Characterization">
        {data?.wells.map((well) => (
          <Panel key={well.id}>
            <div
              title="Well count"
              className="absolute inset-1 inline-flex h-8 w-8 items-center justify-center rounded-full border border-gray-800 bg-white/90 text-xs font-bold text-gray-700"
            >
              {well.count}
            </div>
            <Value className="-mx-3 mb-3 border border-r-0 border-blue-500 bg-blue-200 px-2 py-1 text-center font-bold text-blue-700 shadow">
              {well.status}
            </Value>
            <Label>Injectate Characterization</Label>
            <Value>{handleLink(well.injectateCharacterization, siteId, inventoryId)}</Value>
          </Panel>
        ))}
      </Section>
    </>
  );
};

const Panel = ({ children }) => (
  <div className="relative col-span-6 max-h-72 overflow-auto rounded border bg-white px-3 py-2 shadow md:col-span-2">
    {children}
  </div>
);

function Address({ mailingAddress, city, state, zipCode }) {
  return (
    <>
      <div>{mailingAddress}</div>
      <div>{city && `${city}, ${state} ${zipCode}`} </div>
    </>
  );
}

const LocationDetails = ({ siteId, inventoryId }) => {
  const mapDiv = useRef();
  const groundWaterProtectionZones = useRef(
    new FeatureLayer({
      url: 'https://services2.arcgis.com/NnxP4LZ3zX8wWmP9/ArcGIS/rest/services/Utah_DDW_Groundwater_Source_Protection_Zones/FeatureServer/0',
    })
  );
  const hoverEvent = useRef();

  const [state, setState] = useState({ highlighted: undefined, graphics: [] });

  const { status, data } = useQuery(
    ['inventory', inventoryId],
    () => ky.get(`/api/site/${siteId}/inventory/${inventoryId}`).json(),
    {
      enabled: siteId > 0,
      onError: (error) => onRequestError(error, 'We had some trouble finding this inventory.'),
    }
  );

  const { mapView } = useWebMap(mapDiv, '80c26c2104694bbab7408a4db4ed3382');

  mapView.current?.when(() => {
    if (mapView.current.map.layers.includes(groundWaterProtectionZones.current)) {
      return;
    }

    mapView.current.map.add(groundWaterProtectionZones.current);
  });

  mapView.current?.when(() => {
    if (hoverEvent.current) {
      return;
    }

    hoverEvent.current = mapView.current.on(
      'pointer-move',
      throttle((event) => {
        const { x, y } = event;
        mapView.current.hitTest({ x, y }).then(({ results }) => {
          if (results?.length === 0) {
            setState({ ...state, highlighted: undefined });
            return;
          }

          const id = results[0].graphic.attributes['id'];
          setState({ ...state, highlighted: id });
        });
      }, 100)
    );
  });

  useSitePolygon(mapView, data?.site);
  const wells = useInventoryWells(mapView, data?.wells);

  if (state.graphics.length === 0 && wells?.length > 0) {
    setState({ ...state, graphics: wells });
  }

  return (
    <Section title="Location Details" height="h-screen">
      <div className="md:auto-rows-none col-span-6 grid grid-rows-[.5fr,1.5fr] items-start gap-5 lg:auto-cols-min lg:grid-cols-2 lg:grid-rows-none">
        {status === 'loading' ? <Code /> : <WellTable wells={data?.wells} state={state} />}
        <div className="h-full rounded border shadow" ref={mapDiv}></div>
      </div>
    </Section>
  );
};

const selectGraphic = (id, graphics, selected = undefined) => {
  graphics.map((x) => {
    if (x.attributes.id !== id) {
      x.attributes.selected = false;
      x.symbol = SelectedWellsSymbol.clone();
    }
  });

  const graphic = graphics.filter((x) => x.attributes.id === id)[0];

  graphic.attributes.selected = selected === undefined ? !graphic.attributes.selected : selected;
  graphic.symbol = SelectedWellsSymbol.clone();
};

const Pill = ({ children, status }) => {
  const classes = clsx('text-xs font-medium mx-1 rounded-lg border border-gray-400 shadow-md px-2 py-1', {
    'bg-red-200': status === false,
    'bg-green-200': status === true,
    'bg-gray-100': status === undefined,
  });

  return <span className={classes}>{children}</span>;
};

const WellTable = ({ wells = [], state }) => {
  const [highlighting, setHighlighting] = useState(false);
  const columns = useMemo(
    () => [
      {
        accessor: 'id',
      },
      {
        Header: 'Construction',
        accessor: 'wellName',
        Cell: function id({ row }) {
          return (
            <div className="relative">
              <span
                title="Well count"
                className="mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full border border-gray-800 text-xs font-bold text-gray-700"
              >
                {row.original.count}
              </span>
              {row.original.wellName}
            </div>
          );
        },
      },
      {
        Header: 'Operating Status',
        accessor: 'status',
      },
      {
        Header: 'Ground Water',
        accessor: 'surfaceWaterProtection',
        Cell: function id({ row }) {
          switch (row.original.surfaceWaterProtection) {
            case 'Y+': {
              return (
                <>
                  <Pill status={true}>GWZ</Pill>
                  <Pill status={true}>ARDA</Pill>
                  <span>(Y)</span>
                </>
              );
            }
            case 'Y-': {
              return (
                <>
                  <Pill status={true}>GWZ</Pill>
                  <Pill status={false}>ARDA</Pill>
                  <span>(Y)</span>
                </>
              );
            }
            case 'S': {
              return (
                <>
                  <Pill status={false}>GWZ</Pill>
                  <Pill status={true}>ARDA</Pill>
                  <span>(S)</span>
                </>
              );
            }
            case 'N': {
              return (
                <>
                  <Pill status={false}>GWZ</Pill>
                  <Pill status={false}>ARDA</Pill>
                  <span>(N)</span>
                </>
              );
            }
            default: {
              return 'Unknown';
            }
          }
        },
      },
    ],
    []
  );

  const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } = useTable({
    columns,
    data: wells,
    initialState: {
      hiddenColumns: ['id'],
    },
  });

  return (
    <table {...getTableProps()} className="divide-y divide-gray-200 overflow-auto border">
      <thead className="bg-gray-50">
        {headerGroups.map((headerGroup) => (
          <tr key={headerGroup.index} {...headerGroup.getHeaderGroupProps()}>
            {headerGroup.headers.map((column) => (
              <th
                key={`${headerGroup.index}-${column.id}`}
                {...column.getHeaderProps()}
                className="px-3 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
              >
                {column.render('Header')}
              </th>
            ))}
          </tr>
        ))}
      </thead>
      <tbody {...getTableBodyProps()} className="divide-y divide-gray-200 bg-white">
        {rows.map((row) => {
          prepareRow(row);

          return (
            <tr
              className={clsx(
                {
                  'bg-blue-100': row.original.id === state.highlighted,
                },
                'hover:bg-blue-100'
              )}
              key={`${row.index}`}
              {...row.getRowProps()}
              onPointerEnter={() => {
                setHighlighting(true);
                selectGraphic(row.original.id, state.graphics, true);
              }}
              onPointerLeave={() => {
                setHighlighting(false);
                selectGraphic(row.original.id, state.graphics, false);
              }}
              onClick={() => {
                if (highlighting) {
                  return;
                }
                selectGraphic(row.original.id, state.graphics);
              }}
            >
              {row.cells.map((cell) => (
                <td
                  key={`${row.index}-${cell.column.id}`}
                  className={clsx(
                    {
                      'font-medium': ['action', 'id'].includes(cell.column.id),
                      'whitespace-nowrap text-right': cell.column.id === 'action',
                    },
                    'px-3 py-2'
                  )}
                  {...cell.getCellProps()}
                >
                  <div className="text-sm text-gray-900">{cell.render('Cell')}</div>
                </td>
              ))}
            </tr>
          );
        })}
      </tbody>
    </table>
  );
};
