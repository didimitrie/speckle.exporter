# speckle.exporter
###GH Speckle Exporter Component
###Check out the [user guide](https://github.com/didimitrie/speckle.exporter/wiki/User-Guide) 
The speckle repo is [here](https://github.com/didimitrie/future.speckle).

You can see for yourself the json files being exported if you unzip the file speckle creates when exporting stuff.

A rough breakdown of the files exported: 
- `static.json` - contains the static geometry (objects that *do not change* if the sliders move)
- `params.json` - contains the metadata, basically the secret sauce that makes the sliders, performance evaluation cirteria, etc.
- `xx,yy,zz.json` - these are named after each combination of input slider values. They contain the *dynamic* geometry.

Any questions shoot [@idid](http://twitter.com/idid)

License: MIT
