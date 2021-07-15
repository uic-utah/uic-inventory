import '@arcgis/core/assets/esri/themes/light/main.css';
import {
  Chrome,
  toast,
  useParams,
  PolygonIcon,
  OkNotToggle,
  PointIcon,
  SelectPolygonIcon,
  useHistory,
} from '../../PageElements';
import { GridHeading, Label, SiteLocationSchema as schema } from '../../FormElements';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import Graphic from '@arcgis/core/Graphic';
import Polygon from '@arcgis/core/geometry/Polygon';
import Viewpoint from '@arcgis/core/Viewpoint';
import { TailwindDartboard } from '../../Dartboard/Dartboard';
import { useContext, useRef, useState, useEffect } from 'react';
import clsx from 'clsx';
import { PinSymbol, PolygonSymbol } from '../../MapElements/MarkerSymbols';
import { enablePolygonDrawing } from '../../MapElements/Drawing';
import { AuthContext } from '../../../AuthProvider';
import { useWebMap, useViewPointZooming, useGraphicManager } from '../../Hooks';
import { useQuery, useMutation } from 'react-query';
import ky from 'ky';

function AddSiteLocation() {
  const { authInfo } = useContext(AuthContext);
  const { siteId } = useParams();
  const history = useHistory();
  const mapDiv = useRef(null);
  const pointAddressClickEvent = useRef(null);
  const parcelClickEvent = useRef(null);
  const siteDrawingEvents = useRef(null);
  const [geocodeSuccess, setGeocodeSuccess] = useState(undefined);
  const [address, setAddress] = useState(false);
  const [siteGeometry, setSiteGeometry] = useState(false);
  const [pointAddressClickEnabled, setPointAddressClickEnabled] = useState(false);
  const [parcelClickEnabled, setParcelClickEnabled] = useState(false);
  const [siteDrawingEnabled, setSiteDrawingEnabled] = useState(false);
  const { status, data } = useQuery(['site', siteId], () => ky.get(`/api/site/${siteId}`).json(), {
    enabled: siteId > 0,
  });
  const { mutate } = useMutation((input) => ky.put('/api/site', { json: input }).json(), {
    onSuccess: () => {
      toast.success('Location added successfully!');
      history.push(`/site/${siteId}/add-well`);
    },
    onError: (error) => {
      // TODO: log error
      console.error(error);

      return toast.error('We had some trouble adding the location');
    },
  });
  const { formState, handleSubmit, setValue } = useForm({
    resolver: yupResolver(schema),
  });

  //* pull isDirty from form state to activate proxy
  const { isDirty } = formState;

  const { mapView } = useWebMap(mapDiv, '80c26c2104694bbab7408a4db4ed3382');
  // zoom map on geocode
  const { setViewPoint } = useViewPointZooming(mapView);
  // manage graphics
  const { setGraphic } = useGraphicManager(mapView);

  useEffect(() => {
    if (status !== 'success') {
      return;
    }

    if (data.geometry) {
      setSiteGeometry(data.geometry);
      setValue('geometry', data.geometry);

      const shape = JSON.parse(data.geometry);
      const geometry = new Polygon({
        type: 'polygon',
        rings: shape.rings,
        spatialReference: shape.spatialReference,
      });

      setGraphic(
        new Graphic({
          geometry: geometry,
          attributes: {},
          symbol: PolygonSymbol,
        })
      );

      setViewPoint(new Viewpoint({ targetGeometry: geometry.centroid, scale: 1500 }));
    }

    if (data.address) {
      setAddress(data.address);
      setValue('address', data.address);
    }
  }, [data, status, setGraphic, setViewPoint, setValue]);

  // activate point clicking for selecting an address
  useEffect(() => {
    // if geocoding was successful then skip choosing the address by a point method
    if (geocodeSuccess) {
      return setPointAddressClickEnabled(false);
    }

    // if the event was disabled clear any existing events
    if (!pointAddressClickEnabled) {
      pointAddressClickEvent.current?.remove();
      pointAddressClickEvent.current = null;

      mapView.current.graphics.removeAll();

      return;
    }

    setParcelClickEnabled(false);
    setSiteDrawingEnabled(false);

    mapView.current.focus();

    // enable clicking on the map to set the address
    pointAddressClickEvent.current = mapView.current.on('immediate-click', (event) => {
      setGraphic(
        new Graphic({
          geometry: event.mapPoint,
          attributes: {},
          symbol: PinSymbol,
        })
      );

      setValue('address', `${Math.round(event.mapPoint.x)}, ${Math.round(event.mapPoint.y)}`);
      setAddress(true);

      if (mapView.current.scale > 10489.34) {
        mapView.current.goTo(new Viewpoint({ targetGeometry: event.mapPoint, scale: 10480 }));
      }
    });

    return () => {
      pointAddressClickEvent.current?.remove();
      pointAddressClickEvent.current = null;
    };
  }, [geocodeSuccess, pointAddressClickEnabled, setValue, mapView, setGraphic]);

  // activate parcel hit test clicking
  useEffect(() => {
    // clean up events when disabled
    if (!parcelClickEnabled) {
      parcelClickEvent.current?.remove();
      parcelClickEvent.current = null;

      setGraphic(null);

      setValue('geometry', null);
      setSiteGeometry(false);

      return;
    }

    // trigger other effects to clean up their events
    setPointAddressClickEnabled(false);
    setSiteDrawingEnabled(false);

    mapView.current.focus();

    parcelClickEvent.current = mapView.current.on('click', (event) => {
      //! stop popup from displaying
      event.preventDefault();

      const parcelLayerIndex = mapView.current.map.layers.items[0];

      mapView.current
        .hitTest(event, {
          include: parcelLayerIndex,
        })
        .then((test) => {
          if (test.results.length < 1) {
            return setGraphic(null);
          }

          const parcelGraphic = test.results[0].graphic;
          parcelGraphic.symbol = PolygonSymbol;

          setGraphic(parcelGraphic);
          setValue('geometry', JSON.stringify(parcelGraphic.geometry.toJSON()));
          setSiteGeometry(true);
        });
    });

    return () => {
      parcelClickEvent.current?.remove();
      parcelClickEvent.current = null;
    };
  }, [parcelClickEnabled, pointAddressClickEnabled, setValue, mapView, setGraphic]);

  // activate polygon site drawing
  useEffect(() => {
    if (!siteDrawingEnabled || siteDrawingEnabled === 'complete') {
      for (let index = 0; index < siteDrawingEvents.current?.length; index++) {
        const event = siteDrawingEvents.current[index];

        event.remove();
      }

      siteDrawingEvents.current = null;

      if (siteDrawingEnabled !== 'complete') {
        mapView.current.graphics.removeAll();

        setValue('geometry', null);
        setSiteGeometry(false);
      }

      return;
    }

    setPointAddressClickEnabled(false);
    setParcelClickEnabled(false);

    const [drawAction, drawingEvent] = enablePolygonDrawing(mapView.current);

    const finishEvent = drawAction.on(['draw-complete'], (event) => {
      setSiteDrawingEnabled('complete');

      setValue(
        'geometry',
        JSON.stringify(
          new Polygon({
            type: 'polygon',
            rings: event.vertices,
            spatialReference: mapView.current.spatialReference,
          }).toJSON()
        )
      );

      setSiteGeometry(true);
    });

    siteDrawingEvents.current = [drawingEvent, finishEvent];
  }, [siteDrawingEnabled, setValue, mapView]);

  const geocode = (result) => {
    if (!result) {
      return geocodeError('No match found');
    }

    setGeocodeSuccess(true);
    setValue('address', result.attributes.InputAddress);
    setAddress(true);

    setGraphic(new Graphic(result));
    setViewPoint(new Viewpoint({ targetGeometry: result.geometry, scale: 1500 }));
  };

  const geocodeError = () => {
    setGraphic(null);

    setGeocodeSuccess(false);
    setViewPoint(null);

    setValue('address', null);
    setAddress(false);
  };

  const addSiteLocation = async (formData) => {
    if (!isDirty) {
      history.push(`/site/${siteId}/add-well`);
      return toast.info("We've got your most current information");
    }

    const input = {
      id: parseInt(authInfo.id),
      siteId: parseInt(siteId),
      ...formData,
      geometry: JSON.stringify(formData.geometry),
    };

    await mutate(input);
  };

  return (
    <main>
      <Chrome>
        <div className="md:grid md:grid-cols-3 md:gap-6">
          <GridHeading text="Site Location" subtext="Set the address and polygon for your site">
            <p className="mb-3">
              First, find your site location by it&apos;s address. If you don&apos;t have an address{' '}
              <button type="primary" onClick={() => setGeocodeSuccess(false)}>
                skip
              </button>{' '}
              this step.
            </p>
            <div
              className={clsx('px-4 py-5 ml-4 border rounded transition hover:opacity-100', {
                'opacity-25': address,
              })}
            >
              <TailwindDartboard
                pointSymbol={PinSymbol}
                events={{ success: geocode, error: geocodeError }}
                apiKey={import.meta.env.VITE_API_KEY}
                format="esrijson"
              />
            </div>
            {geocodeSuccess === undefined || geocodeSuccess === true ? null : (
              <>
                <p className="my-3">
                  The site address was not found, select a point for the site instead with the tool below.
                </p>
                <div
                  className={clsx('px-4 py-5 ml-4 border flex justify-center rounded transition hover:opacity-100', {
                    'opacity-25': parcelClickEnabled || siteDrawingEnabled,
                  })}
                >
                  <button
                    type="button"
                    className={clsx({ 'bg-blue-800': pointAddressClickEnabled })}
                    onClick={() => setPointAddressClickEnabled(!pointAddressClickEnabled)}
                  >
                    <PointIcon classes="h-6 text-white fill-current" />
                  </button>
                </div>
              </>
            )}
            {geocodeSuccess || address ? (
              <>
                <p className="my-3">
                  Next, select a parcel as the site polygon or draw the site polygon if the parcel cannot be used.
                </p>
                <div className="flex justify-around px-4 py-5 ml-4 border rounded">
                  <button
                    type="button"
                    className={clsx({ 'bg-blue-800': parcelClickEnabled })}
                    onClick={() => setParcelClickEnabled(!parcelClickEnabled)}
                  >
                    <SelectPolygonIcon classes="h-6 text-white fill-current" />
                  </button>
                  <button
                    type="button"
                    className={clsx({ 'bg-blue-800': siteDrawingEnabled })}
                    onClick={() => setSiteDrawingEnabled(!siteDrawingEnabled)}
                  >
                    <PolygonIcon classes="h-6 text-white fill-current" />
                  </button>
                </div>
              </>
            ) : null}
          </GridHeading>
          <div className="md:mt-0 md:col-span-2">
            <div className="mt-6 overflow-hidden shadow sm:rounded-md">
              <div className="bg-white">
                <div className="grid grid-cols-6">
                  <div className="col-span-6">
                    <div className="w-full h-96" ref={mapDiv}></div>
                    <form
                      className="border-t-2 border-gray-50"
                      onSubmit={handleSubmit((data) => addSiteLocation(data))}
                    >
                      <div className="px-4 py-3">
                        <div className="flex justify-around">
                          <div className="flex flex-col justify-items-center">
                            <Label id="address" />
                            <OkNotToggle classes="h-12" status={address} />
                          </div>
                          <div className="flex flex-col justify-items-center">
                            <Label id="site" text="Site Location" />
                            <OkNotToggle classes="h-12" status={siteGeometry} />
                          </div>
                        </div>
                      </div>
                      <div className="px-4 py-3 text-right bg-gray-100 sm:px-6">
                        <button type="submit" disabled={!(address && siteGeometry)}>
                          Next
                        </button>
                      </div>
                    </form>
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

export default AddSiteLocation;
