using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MBHEngine.GameObject;
using BumpSetSpike.Behaviour;

namespace BumpSetSpike.Gameflow
{
    /// <summary>
    /// Helper singleton for awarding points based on skill moves.
    /// </summary>
    public class ScoreManager
    {
        /// <summary>
        /// The different types of moves.
        /// </summary>
        public enum ScoreType
        {
            // Basics
            Jump = 0,
            Spike,
            Net,
            Kabooom,

            // Advanced
            FingerTips,     // Hit the ball at the top of the collision box.
            HighPoint,      // Hit the ball near it's peak.
            LowPoint,       // Hit the ball near it's lowest point.
            FadeAway,       // Hit the ball while moving backwards.
            Upwards,        // Hit the ball while it is still moving upwards.
            Speedy,         // Hit the ball while moving very fast.
            HangTime,       // Hit the ball after being in the air for a while.

            Count,
        }

        /// <summary>
        /// Stores all the information about a single Score Type.
        /// </summary>
        public class ScoreData
        {
            /// <summary>
            /// How many points is performing this trick once worth.
            /// </summary>
            public Int32 mValue;

            /// <summary>
            /// What is the name of the trick?
            /// </summary>
            public String mName;

            /// <summary>
            /// Same as mValue but expressed as a string to avoid GC at runtime.
            /// </summary>
            public String mValueAsString;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="value">How many points is this trick worth?</param>
            public ScoreData(Int32 value)
            {
                mValue = value;
            }
        }

        /// <summary>
        /// Singleton.
        /// </summary>
        private static ScoreManager mInstance;

        /// <summary>
        /// Maps a type of move to a point value.
        /// </summary>
        private Dictionary<Int32, ScoreData> mScoreMapping;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private PointDisplay.SetScoreMessage mSetScoreMsg;
        private HitCountDisplay.SetScoreMessage mSetHitCountMsg;

        /// <summary>
        /// Tracks all the moves used in the current combo, and how many times each was performed.
        /// </summary>
        private Int32[] mCurrentCombo;

        /// <summary>
        /// Constructor.
        /// </summary>
        private ScoreManager()
        {
            mScoreMapping = new Dictionary<Int32, ScoreData>
            {
                { (Int32)ScoreType.Spike,      new ScoreData(10)  },
                { (Int32)ScoreType.Jump,       new ScoreData(5)   },
                { (Int32)ScoreType.Net,        new ScoreData(20)  },
                { (Int32)ScoreType.Kabooom,    new ScoreData(20)  },

                { (Int32)ScoreType.FingerTips, new ScoreData(25)  },
                { (Int32)ScoreType.HighPoint,  new ScoreData(25)  },
                { (Int32)ScoreType.LowPoint,   new ScoreData(40)  },
                { (Int32)ScoreType.HangTime,   new ScoreData(100) },
                { (Int32)ScoreType.FadeAway,   new ScoreData(50)  },
                { (Int32)ScoreType.Upwards,    new ScoreData(75)  },
                { (Int32)ScoreType.Speedy,     new ScoreData(75)  },
            };

            System.Diagnostics.Debug.Assert(mScoreMapping.Count == (Int32)ScoreType.Count);

            for (Int32 i = 0; i < (Int32)ScoreType.Count; i++)
            {
                mScoreMapping[i].mName = ((ScoreType)i).ToString();
                mScoreMapping[i].mValueAsString = mScoreMapping[i].mValue.ToString();
            }
        }

        /// <summary>
        /// Call this before using the class for the first time.
        /// </summary>
        public void Initialize()
        {
            mCurrentCombo = new Int32[(Int32)ScoreType.Count];

            mSetHitCountMsg = new HitCountDisplay.SetScoreMessage();
            mSetScoreMsg = new PointDisplay.SetScoreMessage();
        }

        /// <summary>
        /// Access to the singleton.
        /// </summary>
        public static ScoreManager pInstance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ScoreManager();
                }

                return mInstance;
            }
        }

        /// <summary>
        /// When points are earned, they should be awarded through this function.
        /// </summary>
        /// <param name="score">How manay points are being awarded.</param>
        /// <param name="positionInWorld">Where the points should appear in world space.</param>
        private void UpdateScoreDisplay(Int32 score, Vector2 positionInWorld)
        {
            if (GameModeManager.pInstance.pMode != GameModeManager.GameMode.TrickAttack)
            {
                return;
            }

            GameObject points = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PointDisplay\\PointDisplay");
            points.pPosition = positionInWorld;

            mSetScoreMsg.Reset();
            mSetScoreMsg.mScore_In = score;
            points.OnMessage(mSetScoreMsg);
             
            GameObjectManager.pInstance.Add(points);

            mSetHitCountMsg.mCount_In = CalcScore();

            GameObjectManager.pInstance.BroadcastMessage(mSetHitCountMsg);
        }

        /// <summary>
        /// When points are earned, they should be awarded through this function.
        /// </summary>
        /// <param name="type">The type of move performed which should be awarded points for.</param>
        /// <param name="positionInWorld">Where the points should appear in world space.</param>
        public void AddScore(ScoreType type, Vector2 positionInWorld)
        {
            mCurrentCombo[(Int32)type]++;

            UpdateScoreDisplay(mScoreMapping[(Int32)type].mValue, positionInWorld);
        }

        /// <summary>
        /// Call this when the match is reset so that the total score can be reset too.
        /// </summary>
        public void OnMatchOver()
        {
            for (Int32 i = 0; i < (Int32)ScoreType.Count; i++)
            {
                mCurrentCombo[(Int32)i] = 0;
            }
        }

        /// <summary>
        /// Calculates the current score.
        /// </summary>
        /// <returns>The current score.</returns>
        public Int32 CalcScore()
        {
            Int32 multiplier = 0;

            Int32 score = 0;

            for (Int32 i = 0; i < (Int32)ScoreType.Count; i++)
            {
                Int32 moveCount = mCurrentCombo[(Int32)i];

                if (moveCount > 0)
                {
                    // +1 multiplier for each TYPE of move performed.
                    multiplier++;

                    score += mScoreMapping[(Int32)i].mValue * moveCount;
                }
            }

            return score * multiplier;
        }

        /// <summary>
        /// Calculate how many different tricks the user has performed, which is used as the Score
        /// Multiplier.
        /// </summary>
        /// <returns></returns>
        public Int32 CalMultiplier()
        {
            Int32 multiplier = 0;

            for (Int32 i = 0; i < (Int32)ScoreType.Count; i++)
            {
                Int32 moveCount = mCurrentCombo[(Int32)i];

                if (moveCount > 0)
                {
                    // +1 multiplier for each TYPE of move performed.
                    multiplier++;
                }
            }

            return multiplier;
        }

        public bool IsMovePerformed(ScoreType type)
        {
            return (mCurrentCombo[(Int32)type] > 0);
        }

        /// <summary>
        /// Access to the array storing how much each move type was done.
        /// </summary>
        public Int32[] pCurrentCombo
        {
            get
            {
                return mCurrentCombo;
            }
        }

        /// <summary>
        /// Access to the mapping of moves to how many points they are worth.
        /// </summary>
        public Dictionary<Int32, ScoreData> pScoreMapping
        {
            get
            {
                return mScoreMapping;
            }
        }
    }
}
