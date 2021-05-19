import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Dialog, Transition } from '@headlessui/react';
import { SiteMutation, useMutation } from '../GraphQL';
import { AuthContext } from '../../AuthProvider';
import { GridHeading, NaicsPicker, SelectInput, TextInput, SiteSchema as schema } from '../FormElements';
import { Chrome, toast, useHistory } from '../PageElements';

const ownership = [
  {
    value: 'PB',
    label: 'Private, For-Profit',
  },
  {
    value: 'PN',
    label: 'Private, Not-For-Profit',
  },
  {
    value: 'PF',
    label: 'Private, Farm',
  },
  {
    value: 'PV',
    label: 'Private, Other',
  },
  {
    value: 'FG',
    label: 'Federal Government',
  },
  {
    value: 'SG',
    label: 'State Government',
  },
  {
    value: 'LG',
    label: 'Local Government',
  },
  {
    value: 'OT',
    label: 'Tribal Government',
  },
  {
    value: 'OI',
    label: 'Individual/Household',
  },
  {
    value: 'OR',
    label: 'Other',
  },
];

function CreateSite() {
  const { authInfo } = React.useContext(AuthContext);
  const [createSite] = useMutation(SiteMutation);
  const { formState, handleSubmit, register, setValue } = useForm({
    resolver: yupResolver(schema),
  });
  //! pull isDirty from form state to activate proxy
  const { isDirty } = formState;
  const history = useHistory();
  const [naicsOpen, setNaicsOpen] = React.useState(false);

  const create = async (state, formData) => {
    if (!isDirty) {
      return toast.info("We've got your most current information");
    }

    const keys = Object.keys(state.dirtyFields);
    const input = {
      id: parseInt(authInfo.id),
    };

    for (let key of keys) {
      input[key] = formData[key];
    }

    const { data, error } = await createSite({
      variables: {
        input: { ...input },
      },
    });

    if (error) {
      return toast.error('We had some trouble creating the site');
      // TODO: log error
    }

    toast.success('Site created successfully!');
    history.push(`/site/${data.createSite.site.id}/add-contacts`);
  };

  return (
    <Chrome>
      <form onSubmit={handleSubmit((data) => create(formState, data))}>
        <div className="md:grid md:grid-cols-3 md:gap-6">
          <GridHeading text="Site Details" subtext="Provide some basic information about the site" />
          <div className="mt-5 md:mt-0 md:col-span-2">
            <div className="overflow-hidden shadow sm:rounded-md">
              <div className="px-4 py-5 bg-white sm:p-6">
                <div className="grid grid-cols-6 gap-6">
                  <div className="col-span-6 sm:col-span-3">
                    <TextInput id="name" register={register} errors={formState.errors} />
                  </div>

                  <div className="col-span-6 sm:col-span-3">
                    <SelectInput
                      id="ownership"
                      text="Land ownership at site"
                      items={ownership}
                      register={register}
                      errors={formState.errors}
                    />
                  </div>

                  <div className="self-center col-span-6 text-center sm:col-span-2 sm:row-span-3">
                    <button type="button" className="sm:items-center sm:h-24" onClick={() => setNaicsOpen(true)}>
                      NAICS Code Helper
                    </button>
                  </div>

                  <div className="col-span-6 sm:col-span-4">
                    <TextInput
                      id="naics"
                      text="6-digit NAICS code"
                      register={register}
                      errors={formState.errors}
                      readOnly={true}
                      className="bg-gray-100"
                    />
                  </div>

                  <div className="col-span-6 sm:col-span-4">
                    <TextInput
                      id="naicsTitle"
                      text="Corresponding NAICS title"
                      register={register}
                      errors={formState.errors}
                      readOnly={true}
                      className="bg-gray-100"
                    />
                  </div>

                  <div className="col-span-6 sm:col-span-6">
                    <TextInput
                      id="activity"
                      text="Describe the primary business activity conducted at the site"
                      register={register}
                      errors={formState.errors}
                    />
                  </div>
                </div>
              </div>
              <div className="px-4 py-3 text-right bg-gray-100 sm:px-6">
                <button type="submit" disabled={!isDirty}>
                  Next
                </button>
              </div>
            </div>
          </div>
        </div>
      </form>
      <Transition appear show={naicsOpen} as={React.Fragment}>
        <Dialog
          as="div"
          className="fixed inset-0 z-10 overflow-y-auto"
          open={naicsOpen}
          onClose={() => setNaicsOpen(false)}
        >
          <div className="min-h-screen px-4 text-center">
            <Transition.Child
              as={React.Fragment}
              enter="ease-out duration-300"
              enterFrom="opacity-0"
              enterTo="opacity-100"
              leave="ease-in duration-200"
              leaveFrom="opacity-100"
              leaveTo="opacity-0"
            >
              <Dialog.Overlay className="fixed inset-0 bg-black opacity-30" />
            </Transition.Child>

            <Transition.Child
              as={React.Fragment}
              enter="ease-out duration-300"
              enterFrom="opacity-0 scale-95"
              enterTo="opacity-100 scale-100"
              leave="ease-in duration-200"
              leaveFrom="opacity-100 scale-100"
              leaveTo="opacity-0 scale-95"
            >
              <div className="inline-block w-full p-6 my-8 text-left align-middle transition-all transform bg-white shadow-xl rounded-2xl">
                <Dialog.Title as="h3" className="text-lg font-medium leading-6 text-gray-900">
                  NAICS Code Helper
                </Dialog.Title>

                <NaicsPicker
                  updateWith={(item) => {
                    if (item.code.toString().length === 6) {
                      setNaicsOpen(false);
                    }

                    setValue('naics', item.code, { shouldValidate: true, shouldDirty: true });
                    setValue('naicsTitle', item.value, { shouldValidate: true, shouldDirty: true });
                  }}
                />

                <button type="button" className="mt-4" onClick={() => setNaicsOpen(false)}>
                  Cancel
                </button>
              </div>
            </Transition.Child>
          </div>
        </Dialog>
      </Transition>
    </Chrome>
  );
}

export default CreateSite;
