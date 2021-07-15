import { useEffect, useState } from 'react';

export function useGraphicManager(mapView) {
  const [graphic, setGraphic] = useState(null);

  useEffect(() => {
    mapView.current.graphics.removeAll();
    mapView.current.graphics.add(graphic);
  }, [graphic, mapView]);

  return { graphic, setGraphic };
}
