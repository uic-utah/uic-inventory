import * as yup from 'yup';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import TextInput from './components/FormElements/TextInput';
import SelectInput from './components/FormElements/SelectInput';
import GridHeading from './components/FormElements/GridHeading';
import Chrome from './components/PageElements/Chrome';

const schema = yup.object().shape({
  name: yup.string().max(512).required().label('Name'),
  ownership: yup.string().max(512).required().label('ownership'),
  naics: yup.string().max(512).required().label('NAICS'),
  naicsTitle: yup.string().max(512).required().label('title'),
  activity: yup.string().max(512).required().label('business activity'),
});

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
  const { formState, handleSubmit, register } = useForm({
    resolver: yupResolver(schema),
  });

  return (
    <Chrome>
      <form onSubmit={handleSubmit((data) => console.log(data))}>
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

                  <div className="col-span-6 sm:col-span-3">
                    <TextInput id="naics" text="6-digit NAICS code" register={register} errors={formState.errors} />
                  </div>

                  <div className="col-span-6 sm:col-span-3">
                    <TextInput
                      id="naicsTitle"
                      text="Corresponding NAICS title"
                      register={register}
                      errors={formState.errors}
                    />
                  </div>

                  <div className="col-span-12 sm:col-span-6">
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
                <button type="submit" disabled={!formState.isDirty}>
                  Next
                </button>
              </div>
            </div>
          </div>
        </div>
      </form>
    </Chrome>
  );
}

export default CreateSite;
