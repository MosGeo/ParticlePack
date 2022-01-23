function surfaceArea = meshSurfaceArea(FV)

faces = FV.Faces;
verts = FV.Vertices;

a = verts(faces(:, 2), :) - verts(faces(:, 1), :);
b = verts(faces(:, 3), :) - verts(faces(:, 1), :);
c = cross(a, b, 2);
surfaceArea = 1/2 * sum(sqrt(sum(c.^2, 2)));

end