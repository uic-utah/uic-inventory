import { Chrome } from '../../PageElements';
import { GridHeading } from '../../FormElements';
import { Instagram } from 'react-content-loader';

function AddSiteLocation() {
  return (
    <Chrome>
      <GridHeading text="location" subtext="where are your wells" />
      <div className="flex justify-center">
        <Instagram height={400} animate={false} />
      </div>
    </Chrome>
  );
}

export default AddSiteLocation;
