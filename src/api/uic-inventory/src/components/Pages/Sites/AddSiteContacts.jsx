import { Fragment, useContext, useEffect, useMemo, useRef } from 'react';
import { Dialog, Transition } from '@headlessui/react';
import { useForm } from 'react-hook-form';
import clsx from 'clsx';
import { BulletList } from 'react-content-loader';
import { yupResolver } from '@hookform/resolvers/yup';
import { TrashIcon } from '@heroicons/react/outline';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import ky from 'ky';
import { useTable } from 'react-table';
import { AuthContext } from '../../../AuthProvider';
import { BackButton, Chrome, onRequestError, toast, useParams, Link } from '../../PageElements';
import {
  ContactSchema as schema,
  ErrorMessage,
  ErrorMessageTag,
  FormGrid,
  PageGrid,
  PhoneInput,
  ResponsiveGridColumn,
  Separator,
  SelectInput,
  TextInput,
} from '../../FormElements';
import { useOpenClosed } from '../../Hooks/useOpenClosedHook';

const contactType = [
  {
    value: 'owner_operator',
    label: 'Owner/Operator',
  },
  {
    value: 'facility_owner',
    label: 'Owner',
  },
  {
    value: 'facility_operator',
    label: 'Operator',
  },
  {
    value: 'facility_manager',
    label: 'Facility Manager',
  },
  {
    value: 'legal_rep',
    label: 'Legal Representative',
  },
  {
    value: 'office_rep',
    label: 'Official Representative',
  },
  {
    value: 'contractor',
    label: 'Contractor',
  },
  {
    value: 'project_manager',
    label: 'DEQ Dist Eng/Project Manager',
  },
  {
    value: 'health_dept',
    label: 'Local Health Department',
  },
  {
    value: 'permit_writer',
    label: 'Permit Writer',
  },
  {
    value: 'developer',
    label: 'Developer',
  },
  {
    value: 'other',
    label: 'Other',
  },
];

const valueToLabel = (value) => {
  const item = contactType.find((x) => x.value === value);

  return item?.label ?? value;
};

function AddSiteContacts() {
  const { siteId } = useParams();
  const { authInfo } = useContext(AuthContext);
  const { control, formState, handleSubmit, register, reset, unregister, watch } = useForm({
    resolver: yupResolver(schema),
  });

  const watchContactType = watch('contactType', '');

  const queryClient = useQueryClient();
  const { status, error, data } = useQuery(['contacts', siteId], () => ky.get(`/api/site/${siteId}/contacts`).json(), {
    enabled: siteId ?? 0 > 0 ? true : false,
    onError: (error) => onRequestError(error, 'We had some trouble finding your contacts.'),
  });
  const { mutate } = useMutation((json) => ky.post('/api/contact', { json }), {
    onMutate: async (contact) => {
      await queryClient.cancelQueries(['contacts', siteId]);
      const previousValue = queryClient.getQueryData(['contacts', siteId]);

      queryClient.setQueryData(['contacts', siteId], (old) => ({
        ...old,
        contacts: [...old.contacts, { ...contact, contactType: valueToLabel(contact.contactType) }],
      }));

      return previousValue;
    },
    onSuccess: () => {
      toast.success('Contact created successfully!');

      reset({});
    },
    onSettled: () => {
      queryClient.invalidateQueries(['contacts', siteId]);
    },
    onError: async (error, variables, previousValue) => {
      queryClient.setQueryData(['contacts', siteId], previousValue);
      onRequestError(error, 'We had some trouble creating this contact.');
    },
  });

  //! pull value from form state to activate proxy
  const { isDirty } = formState;

  // set default fields to owner
  useEffect(() => {
    if (!data) {
      return;
    }

    const skipFields = ['access', 'receiveNotifications', 'errors', 'id'];
    const owner = data.owner;

    //! contact already added do not pre-fill
    if (data.contacts?.find((x) => x.email === owner.email)) {
      return;
    }

    const defaults = Object.keys(owner).reduce((object, key) => {
      if (skipFields.includes(key)) {
        return object;
      }

      if (owner[key] === null) {
        object[key] = '';

        return object;
      }

      object[key] = owner[key];

      return object;
    }, {});

    reset({ ...defaults, contactType: undefined });
  }, [data, reset]);

  // handle conditional control registration
  useEffect(() => {
    if (watchContactType === 'other') {
      register('description', { required: true });
    } else {
      unregister('description');
    }
  }, [watchContactType, register, unregister]);

  const create = (formData) => {
    if (!isDirty) {
      return toast.info("We've got your most current information");
    }

    const input = {
      siteId: parseInt(siteId),
      accountId: parseInt(authInfo.id),
    };

    for (let key of Object.keys(formData)) {
      input[key] = formData[key];
    }

    mutate(input);
  };

  return (
    <Chrome loading={status === 'loading'}>
      <PageGrid
        heading="Site Contacts"
        subtext="At least one of the contacts listed must be the owner, owner/operator, or legal representative of the injection well system for which the UIC Well Inventory is being submitted. The owner, owner/operator, or legal representative must be the signatory of the form."
      >
        <div className="min-h-screen mt-5 md:mt-0 md:col-span-2">
          {status === 'loading' && <BulletList style={{ height: '20em' }} />}
          {status !== 'loading' && !error && (
            <div className="flex flex-col">
              <div className="-my-2 overflow-x-auto sm:-mx-6 lg:-mx-8">
                <div className="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
                  <div className="overflow-hidden border-b border-gray-200 shadow sm:rounded-lg">
                    <ContactTable data={data?.contacts} />
                    <div className="px-4 py-3 text-right bg-gray-100 sm:px-6">
                      <Link type="button" to={`/site/${siteId}/add-location`}>
                        Next
                      </Link>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}
          {error && (
            <h1>Something went terribly wrong</h1>
            // Log error
          )}
        </div>
      </PageGrid>

      <Separator />

      <form onSubmit={handleSubmit(create)} className="mt-10 sm:mt-0">
        <PageGrid
          subtext="Provide additional contacts capable of providing reliable information regarding the operation of the facility."
          submit={true}
          submitLabel="Add"
          disabled={!isDirty}
        >
          <FormGrid>
            <ResponsiveGridColumn full={true} half={true}>
              <TextInput id="firstName" control={control} register={register} errors={formState.errors} />
            </ResponsiveGridColumn>

            <ResponsiveGridColumn full={true} half={true}>
              <TextInput id="lastName" register={register} errors={formState.errors} />
            </ResponsiveGridColumn>

            <ResponsiveGridColumn full={true} half={true}>
              <TextInput id="email" type="email" text="Email address" register={register} errors={formState.errors} />
            </ResponsiveGridColumn>

            <ResponsiveGridColumn full={true} half={true}>
              <label htmlFor="phoneNumber" className="block font-medium text-gray-700">
                Phone number
              </label>
              <PhoneInput name="phoneNumber" type="tel" country="US" control={control} rules={{ required: true }} />
              <ErrorMessage name="phoneNumber" errors={formState.errors} as={ErrorMessageTag} />
            </ResponsiveGridColumn>

            <ResponsiveGridColumn full={true} half={true}>
              <TextInput id="organization" register={register} errors={formState.errors} />
            </ResponsiveGridColumn>

            <ResponsiveGridColumn full={true} half={true}>
              <SelectInput id="contactType" items={contactType} register={register} errors={formState.errors} />
            </ResponsiveGridColumn>

            {watchContactType === 'other' && (
              <ResponsiveGridColumn full={true}>
                <TextInput id="description" register={register} errors={formState.errors} />
              </ResponsiveGridColumn>
            )}

            <ResponsiveGridColumn full={true}>
              <TextInput id="mailingAddress" text="Street address" register={register} errors={formState.errors} />
            </ResponsiveGridColumn>

            <ResponsiveGridColumn full={true} third={true}>
              <TextInput id="city" register={register} errors={formState.errors} />
            </ResponsiveGridColumn>

            <ResponsiveGridColumn full={true} half={true} third={true}>
              <TextInput id="state" register={register} errors={formState.errors} />
            </ResponsiveGridColumn>

            <ResponsiveGridColumn full={true} half={true} third={true}>
              <TextInput id="zipCode" text="ZIP" register={register} errors={formState.errors} />
            </ResponsiveGridColumn>
          </FormGrid>
        </PageGrid>
      </form>
      <BackButton />
    </Chrome>
  );
}

function ContactTable({ data }) {
  const { siteId } = useParams();
  const { authInfo } = useContext(AuthContext);
  const [isOpen, { open, close }] = useOpenClosed();
  const deleteContact = useRef();

  const { mutate } = useMutation((json) => ky.delete(`/api/contact`, { json }), {
    onMutate: async (mutationData) => {
      await queryClient.cancelQueries(['contacts', siteId]);
      const previousValue = queryClient.getQueryData(['contacts', siteId]);

      queryClient.setQueryData(['contacts', siteId], (old) => {
        return {
          ...old,
          contacts: old.contacts.filter((x) => x.id !== mutationData.contactId),
        };
      });

      close();

      return previousValue;
    },
    onSuccess: () => {
      toast.success('This contact was removed.');
    },
    onSettled: () => {
      queryClient.invalidateQueries(['contacts', siteId]);
    },
    onError: (error, variables, previousValue) => {
      queryClient.setQueryData(['contacts', siteId], previousValue);
      onRequestError(error, 'We had some trouble deleting this contact.');
    },
  });
  const columns = useMemo(
    () => [
      {
        Header: 'Id',
        accessor: 'id',
      },
      {
        id: 'name',
        Header: 'Name',
        Cell: function name(data) {
          return (
            <>
              <div className="text-sm font-medium text-gray-900">{`${data.row.original.firstName} ${data.row.original.lastName}`}</div>
              <div className="text-sm text-gray-500">{valueToLabel(data.row.original.contactType)}</div>
            </>
          );
        },
      },
      {
        id: 'contact',
        Header: 'Contact',
        Cell: function contact(data) {
          return (
            <>
              <div className="text-sm text-gray-900">{data.row.original.email}</div>
              <div className="text-sm text-gray-500">{data.row.original.phoneNumber}</div>
            </>
          );
        },
      },
      {
        Header: 'Organization',
        accessor: 'organization',
      },
      {
        id: 'address',
        Header: 'Address',
        Cell: function contact(data) {
          return (
            <>
              <div>{data.row.original.mailingAddress}</div>
              <div>{`${data.row.original.city}, ${data.row.original.state} ${data.row.original.zipCode}`} </div>
            </>
          );
        },
      },
      {
        id: 'action',
        Header: 'Action',
        Cell: function action(data) {
          return (
            <TrashIcon
              aria-label="delete site"
              className="w-6 h-6 ml-1 text-red-600 cursor-pointer hover:text-red-900"
              onClick={() => {
                open();
                deleteContact.current = data.row.original.id;
              }}
            />
          );
        },
      },
    ],
    [open]
  );

  const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } = useTable({ columns, data });

  const queryClient = useQueryClient();

  const remove = () =>
    mutate({
      siteId: parseInt(siteId),
      accountId: parseInt(authInfo.id),
      contactId: deleteContact.current,
    });

  return data?.length < 1 ? (
    <div className="flex flex-col items-center">
      <div className="px-5 py-4 m-6">
        <h2 className="mb-1 text-xl font-medium">Create your first contact</h2>
        <p className="text-gray-700">Get started by filling out the form below to add your first site contact.</p>
        <div className="mb-6 text-sm text-center text-gray-900"></div>
      </div>
    </div>
  ) : (
    <>
      <Transition appear show={isOpen} as={Fragment}>
        <Dialog
          as="div"
          open={isOpen}
          onClose={() => {
            close();
            deleteContact.current = null;
          }}
          className="fixed inset-0 z-10 overflow-y-auto"
        >
          <div className="min-h-screen px-4 text-center">
            <Transition.Child
              as={Fragment}
              enter="ease-out duration-300"
              enterFrom="opacity-0"
              enterTo="opacity-100"
              leave="ease-in duration-200"
              leaveFrom="opacity-100"
              leaveTo="opacity-0"
            >
              <Dialog.Overlay className="fixed inset-0 bg-black opacity-30" />
            </Transition.Child>

            <span className="inline-block h-screen align-middle" aria-hidden="true">
              &#8203;
            </span>

            <Transition.Child
              as={Fragment}
              enter="ease-out duration-300"
              enterFrom="opacity-0 scale-95"
              enterTo="opacity-100 scale-100"
              leave="ease-in duration-200"
              leaveFrom="opacity-100 scale-100"
              leaveTo="opacity-0 scale-95"
            >
              <div className="inline-block w-full max-w-md p-6 mx-auto my-48 overflow-hidden text-left align-middle transition-all transform bg-white shadow-xl rounded-2xl">
                <Dialog.Title className="text-lg font-medium leading-6 text-gray-900">
                  Contact Deletion Confirmation
                </Dialog.Title>
                <Dialog.Description className="mt-1">This contact will be permanently deleted</Dialog.Description>

                <p className="mt-1 text-sm text-gray-500">
                  Are you sure you want to delete this contact? This action cannot be undone.
                </p>

                <div className="flex justify-around mt-6">
                  <button type="button" className="bg-indigo-900" onClick={remove}>
                    Yes
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      close();
                      deleteContact.current = null;
                    }}
                  >
                    No
                  </button>
                </div>
              </div>
            </Transition.Child>
          </div>
        </Dialog>
      </Transition>

      <table {...getTableProps()} className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          {headerGroups.map((headerGroup) => (
            <tr key={headerGroup.index} {...headerGroup.getHeaderGroupProps()}>
              {headerGroup.headers.map((column) => (
                <th
                  key={`${headerGroup.index}-${column.id}`}
                  {...column.getHeaderProps()}
                  className="px-3 py-3 text-xs font-medium tracking-wider text-left text-gray-500 uppercase"
                >
                  {column.render('Header')}
                </th>
              ))}
            </tr>
          ))}
        </thead>
        <tbody {...getTableBodyProps()} className="bg-white divide-y divide-gray-200">
          {rows.map((row) => {
            prepareRow(row);

            return (
              <tr key={`${row.index}`} {...row.getRowProps()}>
                {row.cells.map((cell) => (
                  <td
                    key={`${row.index}-${cell.column.id}`}
                    className={clsx(
                      {
                        'font-medium': ['action', 'id'].includes(cell.column.id),
                        'text-right whitespace-nowrap': cell.column.id === 'action',
                      },
                      'px-3 py-4'
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
    </>
  );
}

export default AddSiteContacts;
