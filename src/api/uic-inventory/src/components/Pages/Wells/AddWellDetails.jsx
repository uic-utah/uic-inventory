import { useContext, useEffect, useRef, useState } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import * as yup from 'yup';
import { useQuery, useQueryClient, useMutation } from 'react-query';
import ky from 'ky';
import { ErrorMessage } from '@hookform/error-message';
import Graphic from '@arcgis/core/Graphic';
import Polygon from '@arcgis/core/geometry/Polygon';
import Point from '@arcgis/core/geometry/Point';
import Viewpoint from '@arcgis/core/Viewpoint';
import { PinSymbol, PolygonSymbol } from '../../MapElements/MarkerSymbols';
import { Chrome, onRequestError, toast, useParams } from '../../PageElements';
import { GridHeading, LimitedTextarea, LimitedDropzone, Label } from '../../FormElements';
import { useWebMap, useViewPointZooming, useGraphicManager } from '../../Hooks';
import { AuthContext } from '../../../AuthProvider';
import ErrorMessageTag from '../../FormElements/ErrorMessage';

import '@arcgis/core/assets/esri/themes/light/main.css';

const empty = (val) => {
  return val === undefined || val === null || val === '';
};

function AddWellDetails() {
  const { authInfo } = useContext(AuthContext);
  const { siteId, inventoryId } = useParams();
  const queryClient = useQueryClient();
  const mapDiv = useRef(null);
  const pointAddressClickEvent = useRef(null);
  const hoverEvent = useRef(null);
  const [wellsRemaining, setWellsRemaining] = useState(0);
  const [selectedWells, setSelectedWells] = useState([]);

  // get site and inventory data
  const { status, data } = useQuery(
    ['inventory', inventoryId],
    () => ky.get(`/api/site/${siteId}/inventory/${inventoryId}`).json(),
    {
      enabled: siteId > 0,
      onError: (error) => onRequestError(error, 'We had some trouble finding your wells.'),
    }
  );

  // update wells
  const { mutate } = useMutation((body) => ky.put('/api/well', { body }).json(), {
    onSuccess: () => {
      toast.success('Wells updated successfully!');
      queryClient.invalidateQueries(['inventory', inventoryId]);
    },
    onError: (error) => onRequestError(error, 'We had some trouble updating your wells.'),
  });

  const { control, formState, handleSubmit, register, reset, trigger } = useForm({
    resolver: (resolverData, context) => {
      const errors = {};
      if (
        (empty(resolverData.constructionDetails) && empty(resolverData.constructionDetailsFile)) ||
        (!empty(resolverData.constructionDetails) && !empty(resolverData.constructionDetailsFile))
      ) {
        errors.constructionDetails = { message: 'Choose to type your response or upload a file' };
      } else if (!empty(resolverData.constructionDetails)) {
        try {
          yup.string().max(2500).required().validateSync(resolverData.constructionDetails);
        } catch (error) {
          errors.constructionDetails = { message: error.message };
        }
      } else if (!empty(resolverData.constructionDetailsFile)) {
        try {
          yup
            .mixed()
            .required()
            .test('constructionDetailsFile', 'File is missing a path', (value) => value.path)
            .validateSync(resolverData.constructionDetailsFile);
        } catch (error) {
          errors.constructionDetailsFile = { message: error.message };
        }
      }

      try {
        yup.string().max(2500).optional().validateSync(resolverData.hydrogeologicCharacterization);
      } catch (error) {
        errors.hydrogeologicCharacterization = { message: error.message };
      }

      try {
        yup
          .array(
            yup.object().shape({
              id: yup.number().integer().positive(),
            })
          )
          .min(1)
          .typeError('You must select at least 1 well')
          .validateSync(resolverData.selectedWells);
      } catch (error) {
        errors.selectedWells = { message: error.message };
      }

      if (context.subClass === 5002) {
        if (
          (empty(resolverData.injectateCharacterization) && empty(resolverData.injectateCharacterizationFile)) ||
          (!empty(resolverData.injectateCharacterization) && !empty(resolverData.injectateCharacterizationFile))
        ) {
          errors.injectateCharacterization = { message: 'Choose to type your response or upload a file' };
        } else if (!empty(resolverData.injectateCharacterization)) {
          try {
            yup.string().max(2500).required().validateSync(resolverData.injectateCharacterization);
          } catch (error) {
            errors.injectateCharacterization = { message: error.message };
          }
        } else if (!empty(resolverData.injectateCharacterizationFile)) {
          try {
            yup
              .mixed()
              .required()
              .test('injectateCharacterizationFile', 'File is missing a path', (value) => value.path)
              .validateSync(resolverData.injectateCharacterizationFile);
          } catch (error) {
            errors.injectateCharacterizationFile = { message: error.message };
          }
        }
      }

      return { values: errors ? {} : resolverData, errors: errors };
    },
    context: { subClass: data?.subClass },
  });

  const { append, remove } = useFieldArray({ control, name: 'selectedWells' });

  const { mapView } = useWebMap(mapDiv, '80c26c2104694bbab7408a4db4ed3382');
  // zoom map on geocode
  const { setViewPoint } = useViewPointZooming(mapView);
  // manage graphics
  const { graphic, setGraphic: setPolygonGraphic } = useGraphicManager(mapView);
  const { graphic: wellGraphics, setGraphic: setExistingPointGraphics } = useGraphicManager(mapView);

  // place site polygon
  useEffect(() => {
    if (status !== 'success' || graphic) {
      return;
    }

    const shape = JSON.parse(data.site.geometry);
    const geometry = new Polygon({
      type: 'polygon',
      rings: shape.rings,
      spatialReference: shape.spatialReference,
    });

    setPolygonGraphic(
      new Graphic({
        geometry: geometry,
        attributes: {},
        symbol: PolygonSymbol,
      })
    );

    setViewPoint(new Viewpoint({ targetGeometry: geometry.centroid, scale: 1500 }));
  }, [data, status]);

  // place site wells
  useEffect(() => {
    if (status !== 'success') {
      return;
    }

    const wells = data.wells.map(
      (well) =>
        new Graphic({
          geometry: new Point(JSON.parse(well.geometry)),
          attributes: { id: well.id },
          symbol: PinSymbol,
        })
    );

    setWellsRemaining(wells.length);
    setExistingPointGraphics(wells);
  }, [data, status]);

  useEffect(() => {
    if (pointAddressClickEvent.current || hoverEvent.current) {
      pointAddressClickEvent.current?.remove();
      pointAddressClickEvent.current = null;

      return;
    }
    const opts = {
      include: wellGraphics,
    };

    pointAddressClickEvent.current = mapView.current.on('immediate-click', (event) => {
      mapView.current.hitTest(event, opts).then((response) => {
        if (!response.results.length) {
          return;
        }

        const graphic = response.results[0].graphic;
        const index = selectedWells.findIndex((item) => item.id === graphic.attributes.id);

        if (index > -1) {
          remove(index);
          setSelectedWells(selectedWells.splice(index, 1));

          return;
        }

        const wells = [...selectedWells, graphic.attributes];

        setSelectedWells(wells);
        append(graphic.attributes);
      });
    });

    return () => {
      pointAddressClickEvent.current?.remove();
      pointAddressClickEvent.current = null;
    };
  }, [wellGraphics, append, remove, selectedWells]);

  const updateWells = (submittedData) => {
    const formData = new FormData();

    formData.append('accountId', parseInt(authInfo.id));
    formData.append('inventoryId', parseInt(inventoryId));
    formData.append('siteId', parseInt(siteId));
    submittedData.selectedWells?.forEach((item) => formData.append('selectedWells[]', item.id));

    formData.append('hydrogeologicCharacterization', submittedData.hydrogeologicCharacterization);
    formData.append('constructionDetails', submittedData.constructionDetails);
    formData.append('injectateCharacteristics', submittedData.injectateCharacteristics);

    formData.append('constructionDetailsFile', submittedData.constructionDetailsFile);
    formData.append('injectateCharacteristicsFile', submittedData.injectateCharacteristicsFile);

    mutate(formData);
  };

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
                  <span className="text-5xl font-extrabold text-red-700">
                    {wellsRemaining - (selectedWells?.length ?? 0)}
                  </span>
                  <span className="text-base border-t border-gray-100">
                    <ErrorMessage errors={formState.errors} name="selectedWells" as={ErrorMessageTag} />
                  </span>
                </div>
              </div>
            </div>
          </GridHeading>
          <div className="md:mt-0 md:col-span-2">
            <form onSubmit={handleSubmit((ok) => console.log(ok))}>
              <div className="overflow-hidden shadow sm:rounded-md">
                <div className="bg-white">
                  <div className="grid grid-cols-6">
                    <div className="col-span-6">
                      <div className="w-full border-b-2 h-96 border-gray-50" ref={mapDiv}></div>
                      <section className="flex flex-col gap-2 px-4 py-5">
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
                            control,
                            reset,
                            trigger,
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
                            control,
                            reset,
                            trigger,
                          }}
                          file={{
                            id: 'injectateCharacterizationFile',
                          }}
                        />
                        <div className="md:col-span-2">
                          <Label id="hydrogeologicCharacterization" />
                          <LimitedTextarea
                            id="hydrogeologicCharacterization"
                            rows="5"
                            maxLength={2500}
                            register={register}
                            errors={formState.errors}
                          />
                        </div>
                      </section>
                    </div>
                  </div>
                </div>
                <div className="px-4 py-3 text-right bg-gray-100 sm:px-6">
                  <button type="submit">Next</button>
                </div>
              </div>
            </form>
          </div>
        </div>
      </Chrome>
    </main>
  );
}

export default AddWellDetails;
