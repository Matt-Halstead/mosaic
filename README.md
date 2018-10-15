# mosaic
A very basic photo mosaic builder app.

Given a supplied photo and grid size, crawl over photos sourced asyncronously from a webservice, find those with colour and content matching some region of the target photo, and add to the mosaic.

General approach:
* Subdivide the target image into regions
* Sample each region to get a histogram and perhaps important edges
* Gather candidate images from Imgur: https://apidocs.imgur.com/
* For each candidate, try to find a region that matches a target region within some tolerance
* Assign candidates that match a region as tiles in the complete mosiac.

## Usage

The app is at this time implemented only as a .Net Framework console app.
 
The target image is an embedded resource TestImage.png, in the Mosaic.Console assembly.
 
When you run the app you will see it start querying and downloading from the Imgur API, and saving images here:
```
%TEMP%\mosaic\cells\*.png
```

If you let the app run its course, it should eventually report that no more cells require assignment to the mosaic image.  At this time, or any earlier time, you can press escape to stop.  The mosaic will be built from whatever assigned cells have been processed, and the result will be written to:
```
%TEMP%\mosaic\mosaic-<date and time>.png
```
