﻿// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.Azure.Documents.Spatial;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Types;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;

namespace UnitTestsProject
{
    [TestClass]
    public class TemporalMemoryTest_Syed_Mostain_Ahmed
    {
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// For debugging the tests
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        private static bool areDisjoined<T>(ICollection<T> arr1, ICollection<T> arr2)
        {
            foreach (var item in arr1)
            {
                if (arr2.Contains(item))
                    return false;
            }

            return true;
        }

        private Parameters getDefaultParameters()
        {
            Parameters retVal = Parameters.getTemporalDefaultParameters();
            retVal.Set(KEY.COLUMN_DIMENSIONS, new int[] { 32 });
            retVal.Set(KEY.CELLS_PER_COLUMN, 4);
            retVal.Set(KEY.ACTIVATION_THRESHOLD, 3);
            retVal.Set(KEY.INITIAL_PERMANENCE, 0.21);
            retVal.Set(KEY.CONNECTED_PERMANENCE, 0.5);
            retVal.Set(KEY.MIN_THRESHOLD, 2);
            retVal.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 3);
            retVal.Set(KEY.PERMANENCE_INCREMENT, 0.10);
            retVal.Set(KEY.PERMANENCE_DECREMENT, 0.10);
            retVal.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.0);
            retVal.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            retVal.Set(KEY.SEED, 42);

            return retVal;
        }

        private HtmConfig GetDefaultTMParameters()
        {
            HtmConfig htmConfig = new HtmConfig(new int[] { 32 }, new int[] { 32 })
            {
                CellsPerColumn = 4,
                ActivationThreshold = 3,
                InitialPermanence = 0.21,
                ConnectedPermanence = 0.5,
                MinThreshold = 2,
                MaxNewSynapseCount = 3,
                PermanenceIncrement = 0.1,
                PermanenceDecrement = 0.1,
                PredictedSegmentDecrement = 0,
                Random = new ThreadSafeRandom(42),
                RandomGenSeed = 42
            };

            return htmConfig;
        }


        private Parameters getDefaultParameters(Parameters p, string key, Object value)
        {
            Parameters retVal = p == null ? getDefaultParameters() : p;
            retVal.Set(key, value);

            return retVal;
        }

        /// <summary>
        /// Test the growth of a new dendrite segment when no matching segments are found
        /// </summary>
        [TestMethod]
        public void TestNewSegmentGrowthWhenNoMatchingSegmentFound()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory(); // TM class object
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            int[] activeColumns = { 0 };
            Cell[] activeCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };

            DistalDendrite dd = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(dd, cn.GetCell(4), 0.3);
            cn.CreateSynapse(dd, cn.GetCell(5), 0.3);

            tm.Compute(activeColumns, true);

            // no matching segment should be found, so a new dendrite segment should be grown
            Assert.AreEqual(1, activeCells[0].DistalDendrites.Count);

            DistalDendrite newSegment = activeCells[0].DistalDendrites[0] as DistalDendrite;

            Assert.IsNotNull(newSegment);
            Assert.AreEqual(2, newSegment.Synapses.Count);
        }

        /// <summary>
        /// Verify that no active cell is present in more than one column in 
        /// the output of Temporal Memory Algorithm.
        /// </summary>
        [TestMethod]
        public void TestNoOverlapInActiveCells()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            // Compute active cells for two columns
            int[] activeColumns = { 0, 1 };
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Get active cells for the first column
            var activeCellsColumn0 = cc.ActiveCells
                .Where(cell => cell.Index == activeColumns[0])
                .ToList();

            // Get active cells for the second column
            var activeCellsColumn1 = cc.ActiveCells
                .Where(cell => cell.Index == activeColumns[1])
                .ToList();

            // Check that no cell is active in both columns
            foreach (var cell in activeCellsColumn0)
            {
                Assert.IsFalse(activeCellsColumn1.Contains(cell));
            }
        }

        [TestMethod]
        public void TestTemporalMemoryComputeReturnsWinnerCells()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 2);
            p = getDefaultParameters(p, KEY.MIN_THRESHOLD, 2);
            p.apply(cn);
            tm.Init(cn);

            int[] activeColumns = { 0, 1, 2, 3 };
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(4, winnerCells.Count);
            Assert.AreEqual(0, winnerCells[0].ParentColumnIndex);
            Assert.AreEqual(1, winnerCells[1].ParentColumnIndex);
        }

        [DataTestMethod]
        [DataRow(new int[] { 0, 1, 2, 3 }, 4, new int[] { 0, 1, 2, 3 })]
        [DataRow(new int[] { 4, 5, 6 }, 3, new int[] { 4, 5, 6 })]
        public void TestTemporalMemoryComputeReturnsWinnerCellsWithDataRow(int[] activeColumns, int expectedWinnerCount, int[] expectedWinnerIndices)
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 2);
            p = getDefaultParameters(p, KEY.MIN_THRESHOLD, 2);
            p.apply(cn);
            tm.Init(cn);

            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(expectedWinnerCount, winnerCells.Count);
            for (int i = 0; i < expectedWinnerCount; i++)
            {
                Assert.AreEqual(expectedWinnerIndices[i], winnerCells[i].ParentColumnIndex);
            }
        }

        /// <summary>
        /// create a Connections object with some Cells and Segments, 
        /// and then call the GetLeastUsedCell method with a list of Cells and a Random object. We then assert 
        /// that the Cell returned by the method is the one that we expect (in this case, c3)
        /// </summary>
        [TestMethod]
        public void TestGetLeastUsedCell()
        {
            // Create a Connections object with some Cells and Segments
            TemporalMemory tm = new TemporalMemory();
            Parameters p = getDefaultParameters();
            Connections conn = new Connections();
            p.apply(conn);
            tm.Init(conn);

            Cell c1 = conn.GetCell(1);
            Cell c2 = conn.GetCell(2);
            Cell c3 = conn.GetCell(3);
            DistalDendrite s1 = conn.CreateDistalSegment(c1);
            DistalDendrite s2 = conn.CreateDistalSegment(c1);
            DistalDendrite s3 = conn.CreateDistalSegment(c2);
            DistalDendrite s4 = conn.CreateDistalSegment(c3);
            Synapse syn1 = conn.CreateSynapse(s1, c1, 0.5);
            Synapse syn2 = conn.CreateSynapse(s1, c2, 0.5);
            Synapse syn3 = conn.CreateSynapse(s2, c3, 0.5);
            Synapse syn4 = conn.CreateSynapse(s3, c2, 0.5);
            Synapse syn5 = conn.CreateSynapse(s4, c1, 0.5);

            // Get the least used Cell from a list of Cells
            List<Cell> cells = new List<Cell> { c1, c2, c3 };
            Random random = new Random(42);
            Cell leastUsedCell = TemporalMemory.GetLeastUsedCell(conn, cells, random);

            // Verify that the least used Cell is c3
            Assert.AreEqual(c3.ParentColumnIndex, leastUsedCell.ParentColumnIndex);
            Assert.AreEqual(c3.Index, leastUsedCell.Index);
        }

        /// <summary>
        /// Create expected and actual sets of active cells and compare them to ensure that the correct cells become active
        /// </summary>
        [TestMethod]
        public void TestWhichCellsBecomeActive()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory(); // TM class object
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            // Activate some columns in the input space
            int[] activeColumns = { 0, 1, 2, 3 };
            Cell[] activeCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            // Compute the next state of the TM
            ComputeCycle cycle = tm.Compute(activeColumns, true) as ComputeCycle;

            // Check which cells are active
            HashSet<Cell> expectedActiveCells = new HashSet<Cell>(activeCells);
            HashSet<Cell> actualActiveCells = new HashSet<Cell>(cycle.ActiveCells);
            TestContext.WriteLine("sequence1 ===>>>>> " + string.Join(",", expectedActiveCells));
            TestContext.WriteLine("sequence1 ===>>>>> " + string.Join(",", actualActiveCells));
            // Ensure that the expected and actual sets of active cells are equal
            //Assert.IsTrue(expectedActiveCells.SetEquals(actualActiveCells));
            Assert.IsTrue(expectedActiveCells.All(eac => actualActiveCells.Contains(eac)));
        }

        /// <summary>
        /// Test calculation of dendrite segments which become active in the current cycle
        /// </summary>
        [TestMethod]
        public void TestCalculateActiveSegments()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            int[] activeColumns = { 0, 1, 2 };
            Cell[] activeCells = cn.GetCells(activeColumns);

            TestContext.WriteLine("activeCells ===>>>>> " + string.Join(",", activeCells[2]));

            // Create dendrite segments and synapses for the active cells
            DistalDendrite dd1 = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(dd1, cn.GetCell(4), 0.3);
            cn.CreateSynapse(dd1, cn.GetCell(5), 0.3);

            DistalDendrite dd2 = cn.CreateDistalSegment(activeCells[1]);
            cn.CreateSynapse(dd2, cn.GetCell(6), 0.3);
            cn.CreateSynapse(dd2, cn.GetCell(7), 0.3);

            DistalDendrite dd3 = cn.CreateDistalSegment(activeCells[2]);
            cn.CreateSynapse(dd3, cn.GetCell(8), 0.3);
            cn.CreateSynapse(dd3, cn.GetCell(9), 0.3);

            // Compute current cycle
            ComputeCycle cycle = tm.Compute(activeColumns, true) as ComputeCycle;

            // Assert that the correct dendrite segments are active
            // Assert.AreEqual(3, cycle.ActiveSegments.Count);

            TestContext.WriteLine("sequence1 ===>>>>> " + string.Join(",", cycle.ActiveSegments));

            Assert.IsTrue(cycle.ActiveSegments.Contains(dd1));
            Assert.IsTrue(cycle.ActiveSegments.Contains(dd2));
            Assert.IsTrue(cycle.ActiveSegments.Contains(dd3));
        }


        [DataTestMethod]
        [DataRow(new int[] { 0, 1, 2, 3 }, new int[] { 5 }, new int[] { 0, 1, 2, 3 }, 4)]
        [DataRow(new int[] { 0, 1, 2, 3, 4 }, new int[] { 5 }, new int[] { 0, 1, 2, 4 }, 4)]
        public void TestActiveSegmentGrowSynapsesAccordingToPotentialOverlap(int[] previousActiveColumns, int[] activeColumns, int[] expectedPresynapticCells, int expectedCount)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p = getDefaultParameters(p, KEY.MIN_THRESHOLD, 1);
            p = getDefaultParameters(p, KEY.ACTIVATION_THRESHOLD, 2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p.apply(cn);
            tm.Init(cn);

            // Use 1 cell per column so that we have easy control over the winner cells.
            List<Cell> prevWinnerCells = new List<Cell>();
            foreach (int col in previousActiveColumns)
            {
                prevWinnerCells.Add(cn.GetCell(col));
            }

            DistalDendrite activeSegment = cn.CreateDistalSegment(cn.GetCell(activeColumns[0]));
            cn.CreateSynapse(activeSegment, cn.GetCell(0), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(1), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(2), 0.2);

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(prevWinnerCells.SequenceEqual(cc.WinnerCells));
            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> presynapticCells = new List<Cell>();
            foreach (var syn in activeSegment.Synapses)
            {
                presynapticCells.Add(syn.GetPresynapticCell());
            }

            Assert.IsTrue(presynapticCells.Count == expectedCount);
        }

        [TestMethod]
        public void TestMatchingSegments()
        {
            // Setup
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p.apply(cn);
            TemporalMemory tm = new TemporalMemory();
            tm.Init(cn);

            // Create two segments with the same set of synapses
            DistalDendrite segment1 = cn.CreateDistalSegment(cn.GetCell(0));
            cn.CreateSynapse(segment1, cn.GetCell(1), 0.5);
            cn.CreateSynapse(segment1, cn.GetCell(2), 0.6);

            DistalDendrite segment2 = cn.CreateDistalSegment(cn.GetCell(0));
            cn.CreateSynapse(segment2, cn.GetCell(1), 0.5);
            cn.CreateSynapse(segment2, cn.GetCell(2), 0.6);

            // Activate a set of columns
            int[] activeColumns = { 1, 2, 3 };
            tm.Compute(activeColumns, true);

            // Test if the matching segments are identified
            throw new AssertInconclusiveException("Not fixed.");
            //List<DistalDendrite> matchingSegments = tm.GrowSynapses(cn, tm.(), cn.GetCell(0), 0.5, 2, new Random());
            //Assert.AreEqual(2, matchingSegments.Count);
            //Assert.IsTrue(matchingSegments.Contains(segment1));
            //Assert.IsTrue(matchingSegments.Contains(segment2));
        }



        [TestMethod]
        [TestCategory("Prod")]
        [DataTestMethod]
        [DataRow(0, 2, 3)]
        [DataRow(1, 3, 4)]
        [DataRow(2, 0, 4)]
        [DataRow(3, 1, 4)]
        public void TestDestroyWeakSynapseOnActiveReinforce(int prevActive, int active, int expectedSynapseCount)
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.INITIAL_PERMANENCE, 0.2);
            p = getDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = getDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);
            tm.Init(cn);

            int[] previousActiveColumns = { prevActive };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            int[] activeColumns = { active };
            Cell expectedActiveCell = cn.GetCell(5);

            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[2], 0.5);
            // Weak Synapse
            cn.CreateSynapse(activeSegment, previousActiveCells[3], 0.009);

            tm.Compute(previousActiveColumns, true);
            tm.Compute(activeColumns, true);

            Assert.AreEqual(expectedSynapseCount, activeSegment.Synapses.Count);
        }

        [TestMethod]
        public void TestAdaptSegment_IncreasePermanence()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            
            p.apply(cn);
            tm.Init(cn);
            // Arrange
            Connections conn = new Connections();
            DistalDendrite segment = cn.CreateDistalSegment(cn.GetCell(5));
            Cell presynapticCell = cn.GetCell(0);
            Cell activeCell = cn.GetCell(1);
            Synapse as1 = cn.CreateSynapse(segment, presynapticCell, .5);
            ICollection<Cell> prevActiveCells = new List<Cell> { activeCell };
            double permanenceIncrement = 0.1;
            double permanenceDecrement = 0.05;

            // Act
            TemporalMemory.AdaptSegment(conn, segment, prevActiveCells, permanenceIncrement, permanenceDecrement);

            // Assert
            Assert.AreEqual(0.45, as1.Permanence);
        }

        [TestMethod]
        public void TestAdaptSegment_PrevActiveCellsContainPresynapticCell_IncreasePermanence()
        {
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = getDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);

            p.apply(cn);
            tm.Init(cn);
            
            var segment = new DistalDendrite();
            var cell1 = cn.GetCell(1);
            var cell2 = cn.GetCell(2);
            Synapse as1 = cn.CreateSynapse(segment, cell1, .5);

            // Act
            TemporalMemory.AdaptSegment(cn, segment, new List<Cell> { cell1, cell2 }, 0.2, 0.1);

            // Assert
            Assert.AreEqual(0.7, as1.Permanence);
        }

        [TestMethod]
        public void TestAddingNewSynapseToDistalSegment()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));

            // Act
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.9);

            // Assert
            Assert.IsTrue(dd.Synapses.Contains(s1));
            Assert.AreEqual(0.9, s1.Permanence);
        }

        /// <summary>
        /// TestRemovingSynapseFromDistalSegment: testing the removal of 
        /// Synapses from distal segments
        /// </summary>
        [TestMethod]
        public void TestRemovingSynapseFromDistalSegment()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);

            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.9);
            Synapse s2 = cn.CreateSynapse(dd, cn.GetCell(42), 0.8);

            Assert.AreEqual(2, dd.Synapses.Count);

            // remove s1
            dd.KillSynapse(s1);

            Assert.AreEqual(1, dd.Synapses.Count);
            Assert.IsFalse(dd.Synapses.Contains(s1));
            Assert.IsTrue(dd.Synapses.Contains(s2));
        }

        /// <summary>
        /// TestUpdatingPermanenceOfSynapse: Verify if the algorithm can update
        /// the Permanence value of the Synapse.
        /// </summary>
        [TestMethod]
        public void TestUpdatingPermanenceOfSynapse()
        {
            // Initialize
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);
            tm.Init(cn);


            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.5);

            // Increment permanence
            s1.Permanence += cn.HtmConfig.PermanenceIncrement;
            Assert.AreEqual(0.6, s1.Permanence, 0.1);

            // Decrement permanence
            s1.Permanence -= cn.HtmConfig.PermanenceDecrement;
            Assert.AreEqual(0.5, s1.Permanence, 0.1);
        }


    }
}