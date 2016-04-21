﻿using System.Collections.Generic;
using VRageMath;
using World.GameActors.GameObjects;
using World.Physics;

namespace World.ToyWorldCore
{
    public class SimpleObjectLayer : IObjectLayer
    {
        private List<GameObject> GameObjects { get; set; }

        public LayerType LayerType { get; set; }

        public SimpleObjectLayer(LayerType layerType)
        {
            LayerType = layerType;
            GameObjects = new List<GameObject>();
        }

        public List<GameObject> GetGameObjects(RectangleF rectangle)
        {
            var list = new List<GameObject>();

            foreach (GameObject gameObject in GameObjects)
            {
                IPhysicalEntity physicalEntity = gameObject.PhysicalEntity;
                RectangleF r = new RectangleF();
                RectangleF cover = physicalEntity.CoverRectangle();
                RectangleF.Intersect(ref cover, ref rectangle, out r);
                if (r.Size.Length() > 0f)
                {
                    list.Add(gameObject);
                }
            }

            return list;
        }

        public bool AddGameObject(GameObject gameObject)
        {
            GameObjects.Add(gameObject);
            return true;
        }

        public List<GameObject> GetAllObjects()
        {
            return GameObjects;
        }
    }
}
