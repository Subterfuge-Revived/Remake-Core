# The Map

Within the game, the map wraps along the two edges.
This means that if a sub flies off the right edge of the map, it will reappear
on the left side of the map moving in the same direction. While this implementation is not a sphere
 (it is a tarus/donut), this design makes it very easy for players to interact with the game.
 Specifically, this makes it so that players can scroll the map seamlessly on their device. In order
 to perform the wrapping of values from one side of the map to the other, the library uses a class
 called the `Rft Vector`.

### RftVector

Rft is an acronym for Rectangular Flat Torus.

In order to perform this map wrapping, the `RftVector` class implements the map
wrapping automatically. The RftVector uses the `%` (modulo) operator to ensure that objects on the map
stay within the map boundaries. There are two important things to know when using this class.
 
First: To make use of the `RftVector`, you must initialize the dimensions of its boundary. While
the RftVector indicates a single vector, the class needs to know the dimensions of the map in 
order to wrap the coordinates to the correct location. Without setting the boundary size, an exception
 will be thrown. The map boundary for `RftVector`s are automatically set when a `Game()`
object is generated, however if you have not instantiated a `Game()`, you must specify the boundary of the vector.
This can be done by using the `RftVector.map` static variable. Creating a 200x200 map would be done as follows:
  
 ```cs
// Set the dimentions
RftVector.map = new Rft(200, 200);

// Create a new vector
RftVector myLocation = new RftVector(10, 50);
``` 

Second: It is important to note that with the `RftVector` the origin (0, 0) is at the CENTER 
of the map instead of being in the top left or bottom right corner. This means that in a map of 200x200 the origin (0, 0) is actually at (100, 100) in
the middle of the map. This is because of how the wrapping needs to occur in order for
the `RftVector` to operate the right way. Therefore, a coordinate of (30, 10) with the origin in
the bottom right would actually be a coordinate of `((Width/2)-10, (Height/2)-30)` in Rft.

                |
                |
    ----------------------------
                |
        .       |
                |
 
 ### Location, Position, and movement

While the coordinates may cause some confusion at first, the implementation of the `RftVector`
class makes performing any coordinate map operations extremely easy.
The `RftVector` has operator overloads that allow adding and subtracting two locations together
(vector math). This makes it very easy to automatically wrap a moving object on the map. All
locations in the game are represented with the `RftVector`. You can easily access the `x` and `y`
components of a vector as well as get the magnitude, and a normalized vector. If more functionality is
required, the RftVector also allows converting to a `Vector2` which is a more comprehensive vector library.

```cs
RftVector myPosition = new RftVector(140, 29);
myVector.x // 140
myVector.y // 29
myVector.Magnitude() // The length of the vector
myVector.Normalize() // returns an `RftVector` of magnitude 1 in the same direction

RftVector myDirection = new RftVector(4, -5);

// Easily add the direction to the position to move
// If you move over the map edge, Rft will automatically wrap.
myPosition += myDirection;
```  

When doing vector math, the RftVector will automatically wrap the coordinates.
Consider the following:

```cs 
// Set a 200x200 map size; meaning (100, 100) and (-100, -100) are the map extents
RftVector.map = new Rft(200, 200)

// We have a sub at (0, 95)
RftVector myLocation = new RftVector(0, 95);

// We have a velocity of (0, 10);
RftVector myVelocity = new RftVector(0, 10);

// We update our position based on our velocity:
myLocation = myLocation + myVelocity;
```

At this point, the resulting vector would be (0, 105). However, based on our map boundary the
map extremes are at (0, 100), and 0, -100). Thus the resulting point exceeds the (0, 100) bound
by 5 units. With the RftVector implementation, the resulting `myLocation` variable would have
a value of (0, -95); this is a value of 5 units from the left edge of the map.