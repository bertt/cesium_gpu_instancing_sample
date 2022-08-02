# cesium_gpu_instancing_sample

Sample of creating ext_mesh_gpu_instancing glTF file for use in Cesium viewer.

Specs see https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Vendor/EXT_mesh_gpu_instancing/README.md

1 glTF model (tree.glb) is instanced with random scales and rotations.

## Database

For database setup see https://github.com/Geodan/i3dm.export/blob/main/docs/getting_started.md

## Run 

When running the program, following files are stored in output directory:

- index.html: contains Cesium viewer + reference to tileset.json

- tileset_traffic_signs.json: contains references to subtree files and glb's

- directory 'content': contains result glb's

- directory 'subtrees': contains subtree files

Put the output directory on a webserver and point the browser to the index.html file.

![image](https://user-images.githubusercontent.com/538812/182333886-a37a7e5e-47b8-43d8-85ae-0bc02c01eb7c.png)
