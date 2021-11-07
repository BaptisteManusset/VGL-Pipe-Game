using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PipeData {
    public PipeData() {
        if (directionLinks == null) {
            directionLinks = new DirectionLinks() {
                up = true,
                down = true,
                right = true,
                left = true
            };
        }
    }


    public string name;
    public Pipe pipe = null;

    public Vector2Int position;

    [Serializable]
    public class DirectionLinks {
        public bool up = false;
        public bool down = false;
        public bool right = false;
        public bool left = false;
    }

    public bool isOutput = false;
    public bool isInput = false;
    public bool isFilled = false;


    public DirectionLinks directionLinks;
}