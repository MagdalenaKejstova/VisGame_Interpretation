﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace DataVisualizer{
    /// <summary>
    /// holds tiling infomartion for chart lines
    /// </summary>
    [Serializable]
    public struct MaterialTiling
    {
        [SerializeField]
        public bool EnableTiling;
        [SerializeField]
        public float TileFactor;
        

        public MaterialTiling(bool enable, float value)
        {
            EnableTiling = enable;
            TileFactor = value;
        }
        //public override bool Equals(object obj)
        //{
        //    if (obj is AutoFloat)
        //    {
        //        AutoFloat cast = (AutoFloat)obj;
        //        if (cast.Automatic == true && EnableTiling == true)
        //            return true;
        //        if (cast.Automatic == false && EnableTiling == false && cast.Value == TileFactor)
        //            return true;
        //        return false;
        //    }
        //    return false;
        //}
        //public override int GetHashCode()
        //{
        //    if (EnableTiling == true)
        //        return EnableTiling.GetHashCode();
        //    return TileFactor.GetHashCode();
        //}

}
}
