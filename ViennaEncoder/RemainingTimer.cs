using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RemeiningTimerProject {

    public class RemainingTimer {

        private class TimedValue {
            private long _TimeStamp;
            private double _Value;

            public TimedValue(long TimeStamp, double Value) {
                _TimeStamp = TimeStamp;
                _Value = Value;
            }

            public double Value {
                get {
                    return _Value;
                }
            }

            public long TimeStamp {
                get {
                    return _TimeStamp;
                }
            }
        }

        #region Members

        private object _SyncLock = new object();

        private Stopwatch _SW; // Elapsed time stopwatch
        private LinkedList<TimedValue> _Data; // Collected data
        private double _TargetValue;
        private TimeSpan _WindowDuration;

        private double _LastSlope; // AKA m
        private double _LastYint; // AKA b
        private double _LastCorreletionCoefficeient; // AKA r
        private TimeSpan _LastElapsed;
        private bool _NeedToRecomputeEstimation; // only if there was a change from the last call.

        #endregion

        #region Ctor

        public RemainingTimer() {
            _SW = new Stopwatch();
            _Data = new LinkedList<TimedValue>();
            _TargetValue = 100.0;
            _WindowDuration = new TimeSpan(0, 0, 45); // default window is 45 seconds.
            _NeedToRecomputeEstimation = true;
            _LastSlope = 0.0;
            _LastYint = 0.0;
            _LastCorreletionCoefficeient = 0.0;
        }

        #endregion

        #region Public

        public void Start() {
            lock (_SyncLock) {
                _SW.Start();
            }
        }

        public void Pause() {
            lock (_SyncLock) {
                _SW.Stop();
            }
        }

        public void StopAndReset() {
            lock (_SyncLock) {
                _SW.Stop();
                _Data.Clear();
                _NeedToRecomputeEstimation = true;
                _LastSlope = 0.0;
                _LastYint = 0.0;
                _LastCorreletionCoefficeient = 0.0;
                _SW.Reset();
            }
        }

        public void Mark(double Value) {
            TimedValue TV = new TimedValue(_SW.ElapsedMilliseconds, Value);
            lock (_SyncLock) {
                _Data.AddFirst(TV);
                _NeedToRecomputeEstimation = true;
                ClearOutOfWindowData(); // remove out of window data
            }
        }

        public TimeSpan GetRemainingEstimation() {
            if (_NeedToRecomputeEstimation) {
                lock (_SyncLock) {
                    ClearOutOfWindowData();
                    if (_Data.Count != 0) {

                        ComputeLinearCoefficients(); // compute linear coefficients using linear regression
                        double RemeiningMilliseconds = (_TargetValue - _LastYint) / _LastSlope; //  y = m*x+b --> (y-b)/m = x

                        if (double.IsNaN(RemeiningMilliseconds) || double.IsInfinity(RemeiningMilliseconds)) {
                            _LastElapsed = TimeSpan.MaxValue; // no data ot invalid data
                        } else {
                            _LastElapsed = TimeSpan.FromMilliseconds(RemeiningMilliseconds);
                        }

                        _NeedToRecomputeEstimation = false; // until data list is changed again.
                    }
                }
            }

            if (_LastElapsed != TimeSpan.MaxValue) {
                return _LastElapsed - _SW.Elapsed;
            } else {
                return TimeSpan.MaxValue;
            }
        }

        public double TargetValue {
            get { return _TargetValue; }
            set { _TargetValue = value; }
        }

        /// <summary>
        /// Returns the "r" value which can indicate how "good" is the estimation (the closer to 1.0 the better)
        /// </summary>
        public double Correletion {
            get {
                return _LastCorreletionCoefficeient;
            }
        }

        public TimeSpan WindowDuration {
            get { return _WindowDuration; }
            set { _WindowDuration = value; }
        }

        public override string ToString() {
            TimeSpan TS = GetRemainingEstimation();
            if (TS == TimeSpan.MaxValue) return "Ismeretlen";

            StringBuilder SB = new StringBuilder();
            if (TS.Days != 0) {
                SB.Append(TS.Days);
                SB.Append(".");
            }
            SB.Append(TS.Hours.ToString("D2"));
            SB.Append(":");
            SB.Append(TS.Minutes.ToString("D2"));
            SB.Append(":");
            SB.Append(TS.Seconds.ToString("D2"));
            return SB.ToString();
        }

        #endregion

        #region Private

        private void ClearOutOfWindowData() {
            lock (_SyncLock) {
                if (_Data.Count == 0) return;
                while (_Data.First.Value.TimeStamp - _Data.Last.Value.TimeStamp > (long)_WindowDuration.TotalMilliseconds) {

                    _Data.RemoveLast(); // Don't need this any more.
                    _NeedToRecomputeEstimation = true;

                    if (_Data.Count == 0) return;
                }
            }
        }

        private void ComputeLinearCoefficients() {

            double SumTime = 0.0;
            double SumValue = 0.0;
            double SumValueTime = 0.0;
            double SumTime2 = 0.0;
            double SumValue2 = 0.0;
            int N = 0;

            lock (_SyncLock) {
                IEnumerator<TimedValue> E = _Data.GetEnumerator();
                while (E.MoveNext()) {
                    double D = (double)E.Current.TimeStamp;
                    double Val = E.Current.Value;
                    SumTime += D;
                    SumTime2 += D * D;
                    SumValue += Val;
                    SumValue2 += Val * Val;
                    SumValueTime += Val * D;
                    N++;
                }
            }

            if (N == 0) { // no data at all
                _LastSlope = 0.0;
                _LastYint = 0.0;
                _LastCorreletionCoefficeient = 0.0;
                return;
            }

            double Sum2Time = SumTime * SumTime;
            double Sum2Value = SumValue * SumValue;
            double Ndouble = (double)N;

            _LastSlope = ((Ndouble * SumValueTime) - (SumTime * SumValue)) / ((Ndouble * SumTime2) - Sum2Time);
            _LastYint = (SumValue - _LastSlope * SumTime) / Ndouble;
            _LastCorreletionCoefficeient = ((Ndouble * SumValueTime) - (SumTime * SumValue)) / Math.Sqrt(((Ndouble * SumTime2) - Sum2Time) * ((Ndouble * SumValue2) - Sum2Value));
        }

        #endregion

    }

}
