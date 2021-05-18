import { AuthContext } from './AuthProvider';
import { BulletList } from 'react-content-loader';
import { ContactMutation } from './GraphQL';
import { ContactQuery } from './GraphQL';
import { ContactSchema } from './Schema';
import { ErrorMessage } from '@hookform/error-message';
import { toast } from 'react-toastify';
import { useForm } from 'react-hook-form';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation } from 'graphql-hooks';
import { yupResolver } from '@hookform/resolvers/yup';
import Chrome from './components/PageElements/Chrome';
import ErrorMessageTag from './components/FormElements/ErrorMessage';
import GridHeading from './components/FormElements/GridHeading';
import PhoneInput from 'react-phone-number-input/react-hook-form-input';
import TextInput from './components/FormElements/TextInput';
import SelectInput from './components/FormElements/SelectInput';

const contactType = [
  {
    value: 'OWNER_OPERATOR',
    label: 'Owner/Operator',
  },
  {
    value: 'FACILITY_OWNER',
    label: 'Owner',
  },
  {
    value: 'FACILITY_OPERATOR',
    label: 'Operator',
  },
  {
    value: 'FACILITY_MANAGER',
    label: 'Facility Manager',
  },
  {
    value: 'LEGAL_REP',
    label: 'Legal Representative',
  },
  {
    value: 'OFFICE_REP',
    label: 'Official Representative',
  },
  {
    value: 'CONTRACTOR',
    label: 'Contractor',
  },
  {
    value: 'PROJECT_MANAGER',
    label: 'DEQ Dist Eng/Project Manager',
  },
  {
    value: 'HEALTH_DEP',
    label: 'Local Health Department',
  },
  {
    value: 'PERMIT_WRITER',
    label: 'Permit Writer',
  },
  {
    value: 'DEVELOPER',
    label: 'Developer',
  },
  {
    value: 'OTHER',
    label: 'Other',
  },
];

function AddContacts() {
  const { siteId } = useParams();
  const [createContact] = useMutation(ContactMutation);
  const { control, formState, handleSubmit, register, reset } = useForm({
    resolver: yupResolver(ContactSchema),
  });
  const { loading, error, data, refetch } = useQuery(ContactQuery, { variables: { id: parseInt(siteId) } });

  // set default fields to owner
  React.useEffect(() => {
    if (data?.siteById) {
      const defaults = data?.siteById.owner;

      //! contact already added do not pre-fill
      if (data?.siteById.contacts.find((x) => x.email == defaults.email)) {
        return;
      }

      for (let name in defaults) {
        if (defaults.hasOwnProperty(name) && defaults[name] === null) {
          defaults[name] = '';
        }
      }

      reset(defaults);
    }
  }, [data, reset]);

  const create = async (state, formData) => {
    if (!state.isDirty) {
      return toast.info("We've got your most current information");
    }

    const input = {
      id: parseInt(siteId),
    };

    for (let key of Object.keys(formData)) {
      input[key] = formData[key];
    }

    const { error } = await createContact({
      variables: {
        input: { ...input },
      },
    });

    if (error) {
      return toast.error('We had some trouble creating the contact');
      // TODO: log error
    }

    refetch();
    toast.success('Contact created successfully!');
  };

  return (
    <Chrome loading={loading}>
      <form onSubmit={handleSubmit((data) => create(formState, data))}>
        <div className="md:grid md:grid-cols-3 md:gap-6">
          <GridHeading
            text="Site Contacts"
            subtext="At least one of the contacts listed must be the owner/operator or legal representative of the owner/operator of the injection well system for which the UIC Inventory Information is being submitted. The owner/operator or the legal representative must be the signatory for the form."
          />
          <div className="min-h-screen mt-5 md:mt-0 md:col-span-2">
            {loading && <BulletList style={{ height: '20em' }} />}
            {(!loading || !error) && <ContactsTable data={data?.siteById.contacts} />}
            {error && (
              <h1>Something went terribly wrong</h1>
              // Log error
            )}
          </div>
        </div>
      </form>
      <form onSubmit={handleSubmit((data) => create(formState, data))} className="mt-10 sm:mt-0">
        <div className="hidden sm:block" aria-hidden="true">
          <div className="py-5">
            <div className="border-t border-gray-200" />
          </div>
        </div>

        <div className="md:grid md:grid-cols-3 md:gap-6">
          <GridHeading
            text=""
            subtext="Provide additional contacts capable of providing reliable information regarding the operation of the facility."
          />
          <div className="mt-5 md:mt-0 md:col-span-2">
            <div className="overflow-hidden shadow sm:rounded-md">
              <div className="px-4 py-5 bg-white sm:p-6">
                <div className="grid grid-cols-6 gap-6">
                  <div className="col-span-6 sm:col-span-3">
                    <TextInput id="firstName" control={control} register={register} errors={formState.errors} />
                  </div>

                  <div className="col-span-6 sm:col-span-3">
                    <TextInput id="lastName" register={register} errors={formState.errors} />
                  </div>

                  <div className="col-span-6 sm:col-span-3">
                    <TextInput
                      id="email"
                      type="email"
                      text="Email address"
                      register={register}
                      errors={formState.errors}
                    />
                  </div>

                  <div className="col-span-6 sm:col-span-3">
                    <label htmlFor="phoneNumber" className="block font-medium text-gray-700">
                      Phone number
                    </label>
                    <PhoneInput
                      name="phoneNumber"
                      type="tel"
                      country="US"
                      control={control}
                      rules={{ required: true }}
                    />
                    <ErrorMessage errors={formState.errors} name="phoneNumber" as={ErrorMessageTag} />
                  </div>

                  <div className="col-span-3">
                    <TextInput id="organization" register={register} errors={formState.errors} />
                  </div>

                  <div className="col-span-3">
                    <SelectInput id="contactType" items={contactType} register={register} errors={formState.errors} />
                  </div>

                  <div className="col-span-6">
                    <TextInput id="description" register={register} errors={formState.errors} />
                  </div>

                  <div className="col-span-6">
                    <TextInput
                      id="mailingAddress"
                      text="Street address"
                      register={register}
                      errors={formState.errors}
                    />
                  </div>

                  <div className="col-span-6 sm:col-span-6 lg:col-span-2">
                    <TextInput id="city" register={register} errors={formState.errors} />
                  </div>

                  <div className="col-span-6 sm:col-span-3 lg:col-span-2">
                    <TextInput id="state" register={register} errors={formState.errors} />
                  </div>

                  <div className="col-span-6 sm:col-span-3 lg:col-span-2">
                    <TextInput id="zipCode" text="ZIP" register={register} errors={formState.errors} />
                  </div>
                  <button type="submit">Add</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </form>
    </Chrome>
  );
}

function ContactsTable({ data }) {
  return (
    <div className="flex flex-col">
      <div className="-my-2 overflow-x-auto sm:-mx-6 lg:-mx-8">
        <div className="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
          <div className="overflow-hidden border-b border-gray-200 shadow sm:rounded-lg">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th
                    scope="col"
                    className="px-3 py-3 text-xs font-medium tracking-wider text-left text-gray-500 uppercase"
                  >
                    Name
                  </th>
                  <th
                    scope="col"
                    className="px-3 py-3 text-xs font-medium tracking-wider text-left text-gray-500 uppercase"
                  >
                    Contact
                  </th>
                  <th
                    scope="col"
                    className="px-3 py-3 text-xs font-medium tracking-wider text-left text-gray-500 uppercase"
                  >
                    Organization
                  </th>
                  <th
                    scope="col"
                    className="px-3 py-3 text-xs font-medium tracking-wider text-left text-gray-500 uppercase"
                  >
                    Address
                  </th>
                  <th scope="col" className="relative px-6 py-3">
                    <span className="sr-only">Edit</span>
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {data?.length > 0 ? (
                  data.map((item) => (
                    <tr key={item.email}>
                      <td className="px-3 py-4 whitespace-nowrap">
                        <div className="text-sm font-medium text-gray-900">{`${item.firstName} ${item.lastName}`}</div>
                        <div className="text-sm text-gray-500">{item.contactType}</div>
                      </td>
                      <td className="px-3 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-900">{item.email}</div>
                        <div className="text-sm text-gray-500">{item.phoneNumber}</div>
                      </td>
                      <td className="px-3 py-4">
                        <div className="text-sm text-gray-900">{item.organization}</div>
                      </td>
                      <td className="px-3 py-4 text-sm text-gray-500 whitespace-nowrap">
                        <div>{item.mailingAddress}</div>
                        <div>{`${item.city} ${item.state} ${item.zipCode}`} </div>
                      </td>
                      <td className="px-3 py-4 text-sm font-medium text-right whitespace-nowrap">
                        <a href="#" className="text-indigo-600 hover:text-indigo-900">
                          Edit
                        </a>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan="5" className="px-3 py-4">
                      <div className="text-sm text-center text-gray-900">No contacts have been added yet</div>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}

export default AddContacts;
