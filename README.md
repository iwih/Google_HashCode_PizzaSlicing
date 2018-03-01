# Goolge Hash Code 2018 - Training Round
Team: root.cake
Happy Coding <3

# Description & Approach
This app will check every possible slicing combination for any pizza, and then record the one of highest score, aka, highest "TotalSlicingCellsCount".

# Input File
Uncomment one -only one- of the lines 62, 63, 64, or 65 in class Program.cs according to what pizza you want to slice. Do that before building the app.

# Output Files
For every highest reached slicing-combination, the application will generate the following files:
1) [Pizza Input File Name]__sliced-pizzaCell-ultimate-slicing-[TotalSlicingCellsCount].out => array of pizza after slicing.
2) [Pizza Input File Name]__ultimate-slicing-[TotalSlicingCellsCount].out => output file of the slicing-combination, according to required format by Google HashCode 2018.
3) [Pizza Input File Name]__ultimate-slicing-[TotalSlicingCellsCount].bmp => Bitmap image representing the pizza after slicing, each cell = 2 x 2 pixels.

# RAM Requirements
The app will need a huge memory to process the big.in pizza, specifically "~200 GB". Yes that is true, I used my SSD as a page-file to backup the RAM.
While for medium.in pizza, it will need about "~4 GB". But, it will not drain large memory for small.in and example.in pizzas.

# Time Of Processing
Processing both of example.in & small.in last for less than a minute on my PC setup. But, for medium.in & big.in, it will not end before days!

# Processing Accelerating
The only way to accelerate the processing speed of this app, is use parallel processing, and because I used algorithm recursion, it was a pain to manage that.
In fact, I managed to parallel-process example.in & small.in, but, for the other bigger two, it was horrible! and no event was hit by the thread. Please, if you have a reasonable method to parallel the application, tell me.

# Peace.. <3 & Happy Coding