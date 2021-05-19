import { BulletList } from 'react-content-loader';
import { ContactMutation, ContactQuery, useQuery, useMutation } from '../../GraphQL';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Chrome, toast, useParams, Link } from '../../PageElements';
import {
  ContactSchema as schema,
  ErrorMessage,
  ErrorMessageTag,
  GridHeading,
  PhoneInput,
  SelectInput,
  TextInput,
} from '../../FormElements';

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

const valueToLabel = (value) => {
  const item = contactType.find((x) => x.value === value);

  return item?.label ?? value;
};

function AddSiteContacts() {
  const { siteId } = useParams();
  const [createContact] = useMutation(ContactMutation);
  const { control, formState, handleSubmit, register, reset, unregister } = useForm({
    resolver: yupResolver(schema),
  });
  const { loading, error, data, refetch } = useQuery(ContactQuery, { variables: { id: parseInt(siteId) } });
  const [optional, setOptional] = React.useState(false);
  //! pull isDirty from form state to activate proxy
  const { isDirty } = formState;

  // set default fields to owner
  React.useEffect(() => {
    if (data?.siteById) {
      let defaults = data?.siteById.owner;

      //! contact already added do not pre-fill
      if (data?.siteById.contacts.find((x) => x.email == defaults.email)) {
        return;
      }

      for (let name in defaults) {
        if (defaults.hasOwnProperty(name) && defaults[name] === null) {
          defaults[name] = '';
        }
      }

      defaults = { ...defaults, contactType: undefined };
      console.log(defaults);

      reset(defaults);
    }
  }, [data, reset]);

  // handle conditional control registration
  React.useEffect(() => {
    if (optional) {
      register('description', { required: true });
    } else {
      unregister('description');
    }
  }, [optional]);

  const create = async (formData) => {
    if (!isDirty) {
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
    reset({});
  };

  return (
    <Chrome loading={loading}>
      <div className="md:grid md:grid-cols-3 md:gap-6">
        <GridHeading
          text="Site Contacts"
          subtext="At least one of the contacts listed must be the owner/operator or legal representative of the owner/operator of the injection well system for which the UIC Inventory Information is being submitted. The owner/operator or the legal representative must be the signatory for the form."
        />
        <div className="min-h-screen mt-5 md:mt-0 md:col-span-2">
          {loading && <BulletList style={{ height: '20em' }} />}
          {!loading && !error && (
            <>
              <ContactsTable data={data?.siteById.contacts} />
              <div className="px-4 py-3 text-right bg-gray-100 sm:px-6">
                <Link type="button" to={`/site/${siteId}/add-location`}>
                  Next
                </Link>
              </div>
            </>
          )}
          {error && (
            <h1>Something went terribly wrong</h1>
            // Log error
          )}
        </div>
      </div>
      <form onSubmit={handleSubmit(create)} className="mt-10 sm:mt-0">
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
                    <ErrorMessage name="phoneNumber" errors={formState.errors} as={ErrorMessageTag} />
                  </div>

                  <div className="col-span-3">
                    <TextInput id="organization" register={register} errors={formState.errors} />
                  </div>

                  <div className="col-span-3">
                    <SelectInput
                      id="contactType"
                      items={contactType}
                      register={register}
                      errors={formState.errors}
                      onUpdate={(event) => setOptional(event.target.value?.toLowerCase() === 'other')}
                    />
                  </div>

                  {optional ? (
                    <div className="col-span-6">
                      <TextInput id="description" register={register} errors={formState.errors} />
                    </div>
                  ) : null}

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
                </div>
              </div>
              <div className="px-4 py-3 text-right bg-gray-100 sm:px-6">
                <button type="submit">Add</button>
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
          <div className="overflow-hidden border border-b-0 border-gray-200 shadow sm:rounded-t-lg">
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
                    <tr key={item.id}>
                      <td className="px-3 py-4 whitespace-nowrap">
                        <div className="text-sm font-medium text-gray-900">{`${item.firstName} ${item.lastName}`}</div>
                        <div className="text-sm text-gray-500">{valueToLabel(item.contactType)}</div>
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
                        <span className="text-indigo-600 hover:text-indigo-900">Edit</span>
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

export default AddSiteContacts;