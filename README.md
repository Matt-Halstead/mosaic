# mosaic
Idea for a photo mosaic builder app

Given a supplied photo and grid size, crawl over photos sourced asyncronously from a webservice, find those with colour and content matching some region of the target photo, and add to the mosaic.

General approach:
* Subdivide the target image into regions
* Sample each region to get a histogram and perhaps important edges
* Gather candidate images from Imgur: https://apidocs.imgur.com/
* For each candidate, try to find a region that matches a target region within some tolerance
* Assign candidates that match a region as tiles in the complete mosiac.
