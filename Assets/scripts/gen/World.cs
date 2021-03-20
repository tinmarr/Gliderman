using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    long seed = 1;
    /**
     * Grows a tree at the given location.
     * @param type The tree.
     * @param rand Random number generator.
     * @param x Block x.
     * @param y Block y.
     * @param z Block z.
     * @return Whether the tree successfully spawned.
     */
    public bool placeTree(int type, long seed, int x, int y, int z) 
    {
        return true;
    }
    /**
    * Gets the seed of the world. The seed is used for the random number generator.
    * @return The seed.
    */
    long getSeed() 
    {
        return seed;
    }

}
