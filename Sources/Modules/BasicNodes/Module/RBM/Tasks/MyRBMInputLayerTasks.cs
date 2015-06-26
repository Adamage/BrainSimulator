﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrainSimulator;
using BrainSimulator.NeuralNetwork.Layers;
using BrainSimulator.NeuralNetwork.Tasks;
using BrainSimulator.RBM;
using BrainSimulator.Utils;
using ManagedCuda.BasicTypes;

namespace CustomModels.RBM.Tasks
{

    [Description("EmptyTask"), MyTaskInfo(OneShot = true)]
    public class MyEmptyTask : MyAbstractBackDeltaTask<MyAbstractLayer>
    {
        public MyEmptyTask() { } //parameterless constructor

        public override void Init(int nGPU)
        {
        }

        public override void Execute() //Task execution
        {
        }
    }
}
