function [sphericity, volume, surfaceArea] = sphericityIndex (FV)

volume      =  meshVolume(FV);
surfaceArea =  meshSurfaceArea(FV);
sphericity = pi^(1/3) * (6*volume)^(2/3) /surfaceArea;

end