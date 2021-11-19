using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class RandomExtensions
{
    public static RandomState Save(this System.Random random)
    {
        var binaryFormatter = new BinaryFormatter();
        using (var temp = new MemoryStream())
        {
            binaryFormatter.Serialize(temp, random);
            return new RandomState(temp.ToArray());
        }
    }

    public static System.Random Restore(this RandomState state)
    {
        var binaryFormatter = new BinaryFormatter();
        using (var temp = new MemoryStream(state.State))
        {
            return (System.Random)binaryFormatter.Deserialize(temp);
        }
    }
}

public struct RandomState
{
    public readonly byte[] State;
    public RandomState(byte[] state)
    {
        State = state;
    }
}
