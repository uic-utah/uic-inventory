import { useQuery, useMutation, AccountMutation, AccountQuery } from '../GraphQL';
import { Controller, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Chrome, toast, useParams } from '../PageElements';
import { Facebook } from 'react-content-loader';
import { Switch } from '@headlessui/react';
import { AuthContext } from '../../AuthProvider';
import {
  ErrorMessage,
  ErrorMessageTag,
  GridHeading,
  PhoneInput,
  ProfileSchema as schema,
  TextInput,
} from '../FormElements';

export function Profile() {
  const { id } = useParams();
  const { authInfo } = React.useContext(AuthContext);
  const { loading, error, data, refetch } = useQuery(AccountQuery, { variables: { id: parseInt(id || authInfo.id) } });
  const [updateAccount] = useMutation(AccountMutation);
  const { control, formState, handleSubmit, register, reset } = useForm({
    resolver: yupResolver(schema),
  });
  //! pull isDirty from form state to activate proxy
  const { isDirty } = formState;
  const {
    getValues: notificationValues,
    control: notificationControl,
    handleSubmit: handleNotificationSubmit,
    formState: notificationFormState,
    reset: notificationReset,
  } = useForm({});
  //! pull isDirty from form state to activate proxy
  const { isDirty: isNotificationDirty } = notificationFormState;

  // fill form fields with existing data
  React.useEffect(() => {
    if (data?.accountById) {
      const defaults = data?.accountById;

      for (let name in defaults) {
        if (defaults.hasOwnProperty(name) && defaults[name] === null) {
          defaults[name] = '';
        }
      }

      notificationReset({ ...notificationValues(), receiveNotifications: defaults.receiveNotifications });

      //* remove default prop so dirty is accurate
      delete defaults.receiveNotifications;

      reset(defaults);
    }
  }, [data, notificationReset, reset]);

  const mutateAccount = async (state, updateDefaultValues, formData) => {
    if (!isDirty) {
      return toast.info("We've got your most current information");
    }

    const keys = Object.keys(state.dirtyFields);
    const input = {
      id: parseInt(id || authInfo.id),
    };

    for (let key of keys) {
      input[key] = formData[key];
    }

    const { errors } = await updateAccount({
      variables: {
        input: { ...input },
      },
    });

    if (errors) {
      return toast.error('We had some trouble updating your profile');
      // TODO: log error
    }

    updateDefaultValues(formData);
    refetch();

    toast.success('Profile updated successfully!');
  };

  return (
    <main>
      <Chrome loading={loading}>
        {!loading && !error && (
          <>
            <form onSubmit={handleSubmit((data) => mutateAccount(formState, reset, data))}>
              <div>
                <div className="md:grid md:grid-cols-3 md:gap-6">
                  <GridHeading
                    text="Personal Information"
                    subtext="Use a permanent address where you can receive mail."
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

                          <div className="col-span-6">
                            <TextInput id="organization" register={register} errors={formState.errors} />
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
                        </div>
                      </div>
                      <div className="px-4 py-3 text-right bg-gray-100 sm:px-6">
                        <button type="submit" disabled={!isDirty}>
                          Save
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </form>
            {data?.accountById.access === 'ELEVATED' ? (
              <form
                onSubmit={handleNotificationSubmit((data) =>
                  mutateAccount(notificationFormState, notificationReset, data)
                )}
                className="mt-10 sm:mt-0"
              >
                <div className="hidden sm:block" aria-hidden="true">
                  <div className="py-5">
                    <div className="border-t border-gray-200" />
                  </div>
                </div>

                <div className="md:grid md:grid-cols-3 md:gap-6">
                  <GridHeading text="" subtext="Provide a clear and concise message for the staff" />
                  <div className="mt-5 md:mt-0 md:col-span-2">
                    <div className="overflow-hidden shadow sm:rounded-md">
                      <div className="px-4 py-5 space-y-6 bg-white sm:p-6">
                        <Switch.Group className="flex items-center" as="div">
                          <Switch.Label className="mr-4">Notify me of administrative events</Switch.Label>
                          <span className="sr-only">Enable notifications</span>
                          <Controller
                            control={notificationControl}
                            name="receiveNotifications"
                            render={({ field: { onChange, value, name } }) => (
                              <Switch
                                checked={value}
                                id={name}
                                onChange={onChange}
                                className={`${
                                  value ? 'bg-indigo-600' : 'bg-gray-300'
                                } relative inline-flex items-center flex-shrink-0 h-6 transition-colors duration-200 ease-in-out border-transparent rounded-full cursor-pointer w-11 b-2 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-700`}
                              >
                                <span
                                  className={`${
                                    value ? 'translate-x-5' : 'translate-x-1'
                                  } pointer-events-none inline-block w-5 h-5 transform bg-white rounded-full shadow ease-in-out duration-300 ring-0`}
                                />
                              </Switch>
                            )}
                          />
                        </Switch.Group>
                      </div>
                      <div className="px-4 py-3 text-right bg-gray-100 sm:px-6">
                        <button type="submit" disabled={!isNotificationDirty}>
                          Save
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </form>
            ) : null}
          </>
        )}
        {loading && <Facebook />}
        {error && (
          <h1>Something went terribly wrong</h1>
          // Log error
        )}
      </Chrome>
    </main>
  );
}

export default Profile;
