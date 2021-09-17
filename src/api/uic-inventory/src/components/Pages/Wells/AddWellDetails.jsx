import { useRef } from 'react';
import { useForm } from 'react-hook-form';
import { Chrome } from '../../PageElements';
import { GridHeading, LimitedTextarea, LimitedDropzone, Label } from '../../FormElements';
import { useWebMap } from '../../Hooks';

import '@arcgis/core/assets/esri/themes/light/main.css';

function AddWellDetails() {
  const data = {
    site: {
      naicsTitle: 'a site',
      name: 'Project A',
    },
  };

  const { handleSubmit, register, formState } = useForm({
    //  resolver: yupResolver(schema),
    //  context: { subClass: data?.subClass },
  });

  const mapDiv = useRef(null);
  const { mapView } = useWebMap(mapDiv, '80c26c2104694bbab7408a4db4ed3382');

  return (
    <main>
      <Chrome>
        <div className="grid gap-4 md:grid md:grid-cols-3 md:gap-5">
          <GridHeading
            text="Add Well Details"
            subtext="Select the wells on the map to which the following descriptions apply. You must have a description for all submitted wells. Upload existing plan(s) or provide a narrative description."
            site={data?.site}
          >
            <div className="text-2xl">
              <div className="flex justify-around">
                <div className="flex flex-col text-center justify-items-center">
                  <Label id="wellsRemaining" />
                  <span className="text-5xl font-extrabold text-red-700">15</span>
                </div>
              </div>
            </div>
          </GridHeading>
          <div className="md:mt-0 md:col-span-2">
            <div className="overflow-hidden shadow sm:rounded-md">
              <div className="bg-white">
                <div className="grid grid-cols-6">
                  <div className="col-span-6">
                    <div className="w-full border-b-2 h-96 border-gray-50" ref={mapDiv}></div>
                    <form
                      className="grid grid-cols-2 gap-2 px-4 py-5"
                      onSubmit={handleSubmit((data) => console.log(data))}
                    >
                      <LimitedDropzone
                        textarea={{
                          id: 'constructionDetails',
                          limit: 2500,
                          rows: '5',
                          placeholder: 'Type your response or upload a file',
                        }}
                        forms={{
                          errors: formState.errors,
                          register,
                        }}
                        file={{
                          id: 'constructionDetailsFile',
                        }}
                      />
                      <LimitedDropzone
                        textarea={{
                          id: 'injectateCharacterization',
                          limit: 2500,
                          rows: '5',
                          placeholder: 'Type your response or upload a file',
                        }}
                        forms={{
                          errors: formState.errors,
                          register,
                        }}
                        file={{
                          id: 'injectateCharacterizationFile',
                        }}
                      />
                      <div className="md:col-span-2">
                        <Label id="hydrogeologicCharacterization" />
                        <LimitedTextarea
                          name="hydrogeologicCharacterization"
                          rows={5}
                          limit={2500}
                          register={register}
                          errors={formState.errors}
                        />
                      </div>
                    </form>
                    <div className="px-4 py-3 text-right bg-gray-100 sm:px-6">
                      <button type="submit" onClick={() => {}}>
                        Next
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Chrome>
    </main>
  );
}

export default AddWellDetails;
