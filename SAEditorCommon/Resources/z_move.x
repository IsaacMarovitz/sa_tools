xof 0303txt 0032
template ColorRGBA {
 <35ff44e0-6c7c-11cf-8f52-0040333594a3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
 FLOAT alpha;
}

template ColorRGB {
 <d3e16e81-7835-11cf-8f52-0040333594a3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
}

template Material {
 <3d82ab4d-62da-11cf-ab39-0020af71e433>
 ColorRGBA faceColor;
 FLOAT power;
 ColorRGB specularColor;
 ColorRGB emissiveColor;
 [...]
}

template Frame {
 <3d82ab46-62da-11cf-ab39-0020af71e433>
 [...]
}

template Matrix4x4 {
 <f6f23f45-7686-11cf-8f52-0040333594a3>
 array FLOAT matrix[16];
}

template FrameTransformMatrix {
 <f6f23f41-7686-11cf-8f52-0040333594a3>
 Matrix4x4 frameMatrix;
}

template Vector {
 <3d82ab5e-62da-11cf-ab39-0020af71e433>
 FLOAT x;
 FLOAT y;
 FLOAT z;
}

template MeshFace {
 <3d82ab5f-62da-11cf-ab39-0020af71e433>
 DWORD nFaceVertexIndices;
 array DWORD faceVertexIndices[nFaceVertexIndices];
}

template Mesh {
 <3d82ab44-62da-11cf-ab39-0020af71e433>
 DWORD nVertices;
 array Vector vertices[nVertices];
 DWORD nFaces;
 array MeshFace faces[nFaces];
 [...]
}

template MeshMaterialList {
 <f6f23f42-7686-11cf-8f52-0040333594a3>
 DWORD nMaterials;
 DWORD nFaceIndexes;
 array DWORD faceIndexes[nFaceIndexes];
 [Material <3d82ab4d-62da-11cf-ab39-0020af71e433>]
}

template VertexElement {
 <f752461c-1e23-48f6-b9f8-8350850f336f>
 DWORD Type;
 DWORD Method;
 DWORD Usage;
 DWORD UsageIndex;
}

template DeclData {
 <bf22e553-292c-4781-9fea-62bd554bdd93>
 DWORD nElements;
 array VertexElement Elements[nElements];
 DWORD nDWords;
 array DWORD data[nDWords];
}


Material z_material {
 0.000000;0.000000;1.000000;1.000000;;
 9.999999;
 0.000000;0.000000;0.000000;;
 0.000000;0.000000;0.000000;;
}

Frame z_component {
 

 FrameTransformMatrix {
  1.000000,0.000000,0.000000,0.000000,0.000000,1.000000,0.000000,0.000000,0.000000,0.000000,1.000000,0.000000,0.000000,0.000000,0.000000,1.000000;;
 }

 Mesh z_component {
  109;
  0.070000;0.000000;0.000000;,
  0.065778;1.000000;0.023941;,
  0.065778;0.000000;0.023941;,
  0.070000;1.000000;0.000000;,
  0.053623;1.000000;0.044995;,
  0.053623;0.000000;0.044995;,
  0.035000;1.000000;0.060622;,
  0.035000;0.000000;0.060622;,
  0.012155;1.000000;0.068937;,
  0.012155;0.000000;0.068937;,
  -0.012155;1.000000;0.068937;,
  -0.012155;0.000000;0.068937;,
  -0.035000;1.000000;0.060622;,
  -0.035000;0.000000;0.060622;,
  -0.053623;1.000000;0.044995;,
  -0.053623;0.000000;0.044995;,
  -0.065778;1.000000;0.023941;,
  -0.065778;0.000000;0.023941;,
  -0.070000;1.000000;0.000000;,
  -0.070000;0.000000;0.000000;,
  -0.065778;1.000000;-0.023941;,
  -0.065778;0.000000;-0.023941;,
  -0.053623;1.000000;-0.044995;,
  -0.053623;0.000000;-0.044995;,
  -0.035000;1.000000;-0.060622;,
  -0.035000;0.000000;-0.060622;,
  -0.012155;1.000000;-0.068937;,
  -0.012155;0.000000;-0.068937;,
  0.012155;1.000000;-0.068937;,
  0.012155;0.000000;-0.068937;,
  0.035000;1.000000;-0.060622;,
  0.035000;0.000000;-0.060622;,
  0.053623;1.000000;-0.044995;,
  0.053623;0.000000;-0.044995;,
  0.065778;1.000000;-0.023941;,
  0.065778;0.000000;-0.023941;,
  0.053623;0.000000;-0.044995;,
  0.012155;0.000000;-0.068937;,
  0.035000;0.000000;-0.060622;,
  -0.035000;0.000000;-0.060622;,
  -0.012155;0.000000;-0.068937;,
  -0.065778;0.000000;-0.023941;,
  -0.053623;0.000000;-0.044995;,
  -0.065778;0.000000;0.023941;,
  -0.070000;0.000000;0.000000;,
  -0.035000;0.000000;0.060622;,
  -0.053623;0.000000;0.044995;,
  0.012155;0.000000;0.068937;,
  -0.012155;0.000000;0.068937;,
  0.053623;0.000000;0.044995;,
  0.035000;0.000000;0.060622;,
  0.070000;0.000000;0.000000;,
  0.065778;0.000000;0.023941;,
  0.065778;0.000000;-0.023941;,
  0.065778;1.000000;0.023941;,
  0.035000;1.000000;0.060622;,
  0.053623;1.000000;0.044995;,
  -0.012155;1.000000;0.068937;,
  0.012155;1.000000;0.068937;,
  -0.053623;1.000000;0.044995;,
  -0.035000;1.000000;0.060622;,
  -0.070000;1.000000;0.000000;,
  -0.065778;1.000000;0.023941;,
  -0.053623;1.000000;-0.044995;,
  -0.065778;1.000000;-0.023941;,
  -0.012155;1.000000;-0.068937;,
  -0.035000;1.000000;-0.060622;,
  0.035000;1.000000;-0.060622;,
  0.012155;1.000000;-0.068937;,
  0.065778;1.000000;-0.023941;,
  0.053623;1.000000;-0.044995;,
  0.070000;1.000000;0.000000;,
  0.250000;1.002746;-0.000000;,
  0.000000;1.602746;-0.000000;,
  0.234923;1.002746;0.085505;,
  0.191511;1.002746;0.160697;,
  0.125000;1.002746;0.216506;,
  0.043412;1.002746;0.246202;,
  -0.043412;1.002746;0.246202;,
  -0.125000;1.002746;0.216506;,
  -0.191511;1.002746;0.160697;,
  -0.234923;1.002746;0.085505;,
  -0.250000;1.002746;0.000000;,
  -0.234923;1.002746;-0.085505;,
  -0.191511;1.002746;-0.160697;,
  -0.125000;1.002746;-0.216506;,
  -0.043412;1.002746;-0.246202;,
  0.043412;1.002746;-0.246202;,
  0.125000;1.002746;-0.216506;,
  0.191511;1.002746;-0.160697;,
  0.234923;1.002746;-0.085505;,
  0.191511;1.002746;-0.160697;,
  0.043412;1.002746;-0.246202;,
  0.125000;1.002746;-0.216506;,
  -0.125000;1.002746;-0.216506;,
  -0.043412;1.002746;-0.246202;,
  -0.234923;1.002746;-0.085505;,
  -0.191511;1.002746;-0.160697;,
  -0.234923;1.002746;0.085505;,
  -0.250000;1.002746;0.000000;,
  -0.125000;1.002746;0.216506;,
  -0.191511;1.002746;0.160697;,
  0.043412;1.002746;0.246202;,
  -0.043412;1.002746;0.246202;,
  0.191511;1.002746;0.160697;,
  0.125000;1.002746;0.216506;,
  0.250000;1.002746;-0.000000;,
  0.234923;1.002746;0.085505;,
  0.234923;1.002746;-0.085505;;
  102;
  3;0,1,2;,
  3;1,0,3;,
  3;2,4,5;,
  3;4,2,1;,
  3;5,6,7;,
  3;6,5,4;,
  3;7,8,9;,
  3;8,7,6;,
  3;9,10,11;,
  3;10,9,8;,
  3;11,12,13;,
  3;12,11,10;,
  3;13,14,15;,
  3;14,13,12;,
  3;15,16,17;,
  3;16,15,14;,
  3;17,18,19;,
  3;18,17,16;,
  3;19,20,21;,
  3;20,19,18;,
  3;21,22,23;,
  3;22,21,20;,
  3;23,24,25;,
  3;24,23,22;,
  3;25,26,27;,
  3;26,25,24;,
  3;27,28,29;,
  3;28,27,26;,
  3;29,30,31;,
  3;30,29,28;,
  3;31,32,33;,
  3;32,31,30;,
  3;33,34,35;,
  3;34,33,32;,
  3;35,3,0;,
  3;3,35,34;,
  3;36,37,38;,
  3;37,39,40;,
  3;39,41,42;,
  3;37,41,39;,
  3;41,43,44;,
  3;43,45,46;,
  3;41,45,43;,
  3;45,47,48;,
  3;47,49,50;,
  3;45,49,47;,
  3;41,49,45;,
  3;37,49,41;,
  3;49,51,52;,
  3;37,51,49;,
  3;36,51,37;,
  3;53,51,36;,
  3;54,55,56;,
  3;55,57,58;,
  3;57,59,60;,
  3;55,59,57;,
  3;59,61,62;,
  3;61,63,64;,
  3;59,63,61;,
  3;63,65,66;,
  3;65,67,68;,
  3;63,67,65;,
  3;59,67,63;,
  3;55,67,59;,
  3;67,69,70;,
  3;55,69,67;,
  3;54,69,55;,
  3;71,69,54;,
  3;72,73,74;,
  3;74,73,75;,
  3;75,73,76;,
  3;76,73,77;,
  3;77,73,78;,
  3;78,73,79;,
  3;79,73,80;,
  3;80,73,81;,
  3;81,73,82;,
  3;82,73,83;,
  3;83,73,84;,
  3;84,73,85;,
  3;85,73,86;,
  3;86,73,87;,
  3;87,73,88;,
  3;88,73,89;,
  3;89,73,90;,
  3;90,73,72;,
  3;91,92,93;,
  3;92,94,95;,
  3;94,96,97;,
  3;92,96,94;,
  3;96,98,99;,
  3;98,100,101;,
  3;96,100,98;,
  3;100,102,103;,
  3;102,104,105;,
  3;100,104,102;,
  3;96,104,100;,
  3;92,104,96;,
  3;104,106,107;,
  3;92,106,104;,
  3;91,106,92;,
  3;108,106,91;;

  MeshMaterialList {
   1;
   102;
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0;
   { z_material }
  }

  DeclData {
   1;
   2;0;3;0;;
   327;
   1065353216,
   0,
   3029388670,
   1064341426,
   0,
   1051663684,
   1064341426,
   0,
   1051663683,
   1065353216,
   0,
   3029727592,
   1061428093,
   0,
   1059360187,
   1061428093,
   0,
   1059360187,
   1056964609,
   0,
   1063105495,
   1056964609,
   0,
   1063105495,
   1043452108,
   0,
   1065098333,
   1043452108,
   0,
   1065098333,
   3190935777,
   0,
   1065098332,
   3190935778,
   0,
   1065098332,
   3204448257,
   0,
   1063105495,
   3204448257,
   0,
   1063105495,
   3208911740,
   0,
   1059360188,
   3208911741,
   0,
   1059360187,
   3211825073,
   0,
   1051663688,
   3211825074,
   0,
   1051663686,
   3212836864,
   0,
   849706274,
   3212836864,
   0,
   838945219,
   3211825076,
   0,
   3199147324,
   3211825075,
   0,
   3199147324,
   3208911744,
   0,
   3206843831,
   3208911744,
   0,
   3206843831,
   3204448260,
   0,
   3210589141,
   3204448259,
   0,
   3210589141,
   3190935795,
   0,
   3212581979,
   3190935794,
   0,
   3212581979,
   1043452078,
   0,
   3212581982,
   1043452082,
   0,
   3212581982,
   1056964592,
   0,
   3210589148,
   1056964591,
   0,
   3210589148,
   1061428086,
   0,
   3206843844,
   1061428086,
   0,
   3206843843,
   1064341423,
   0,
   3199147350,
   1064341423,
   0,
   3199147350,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   0,
   1065353216,
   0,
   1064062661,
   1053092942,
   3023518826,
   3024711848,
   1065353216,
   3020550482,
   1063128701,
   1053092942,
   1050780892,
   1060439471,
   1053092942,
   1058530634,
   1055674052,
   1053092942,
   1061987842,
   1042555698,
   1053092942,
   1063827384,
   3190039362,
   1053092943,
   1063827383,
   3203157702,
   1053092944,
   1061987841,
   3207923118,
   1053092943,
   1058530634,
   3210612348,
   1053092944,
   1050780895,
   3211546309,
   1053092943,
   866233797,
   3210612350,
   1053092943,
   3198264534,
   3207923121,
   1053092944,
   3206014279,
   3203157710,
   1053092942,
   3209471487,
   3190039383,
   1053092942,
   3211311031,
   1042555675,
   1053092943,
   3211311033,
   1055674038,
   1053092942,
   3209471494,
   1060439464,
   1053092942,
   3206014289,
   1063128698,
   1053092943,
   3198264555,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0,
   0,
   3212836864,
   0;
  }
 }
}