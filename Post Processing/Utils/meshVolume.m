function volume = meshVolume(FV)

F = FV.Faces;
V = FV.Vertices;

nFaces = size(F, 1);
vols = zeros(nFaces,1);
for i = 1:nFaces
    currentFace = F(i,:);
    v1 = V(currentFace(1),:); 
    v2 = V(currentFace(2),:); 
    v3 = V(currentFace(3),:); 
    vols(i) = SignedVolumeOfTriangle(v1, v2, v3);
end
volume =  abs(sum(vols));

end