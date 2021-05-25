import '@arcgis/core/assets/esri/themes/light/main.css';
import { Chrome } from '../../PageElements';
import { GridHeading } from '../../FormElements';
import MapView from '@arcgis/core/views/MapView';
import WebMap from '@arcgis/core/WebMap';
import { TailwindDartboard } from '@agrc/dart-board';

function AddSiteLocation() {
  const mapDiv = React.useRef(null);
  const webMap = React.useRef(null);
  const mapView = React.useRef(null);

  React.useEffect(() => {
    if (mapDiv.current) {
      webMap.current = new WebMap({
        portalItem: {
          id: 'aa1d3f80270146208328cf66d022e09c',
        },
      });

      mapView.current = new MapView({
        container: mapDiv.current,
        map: webMap.current,
      });
    }

    return () => {
      mapView.current.destroy();
      webMap.current.destroy();
    };
  }, []);

  return (
    <main>
      <Chrome>
        <div className="md:grid md:grid-cols-3 md:gap-6">
          <GridHeading text="Site Location" subtext="Set the address and polygon for your site">
            <TailwindDartboard apiKey={import.meta.env.VITE_API_KEY} />
            <div>----</div>
            <div>if no geocode select a point for the address</div>
            <div>now draw the site polygon or select a parcel as the polygon</div>
            <div>submit</div>
          </GridHeading>
          <div className="mt-5 md:mt-0 md:col-span-2">
            <div className="overflow-hidden shadow sm:rounded-md">
              <div className="bg-white">
                <div className="grid grid-cols-6">
                  <div className="col-span-6">
                    <div className="w-full h-96" ref={mapDiv}></div>
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
