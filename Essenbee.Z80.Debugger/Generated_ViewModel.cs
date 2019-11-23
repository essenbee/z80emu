using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;




namespace Essenbee.Z80.Debugger
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        readonly Dispatcher _dispatcher;

        public event PropertyChangedEventHandler PropertyChanged;

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: ProgramCounter (string)
        // --------------------------------------------------------------------
        string _ProgramCounter = default;

        void Raise_ProgramCounter ()
        {
          OnPropertyChanged ("ProgramCounter");
        }

        public string ProgramCounter
        {
            get { return _ProgramCounter; }
            set
            {
                if (_ProgramCounter == value)
                {
                    return;
                }

                var prev = _ProgramCounter;

                _ProgramCounter = value;

                Changed_ProgramCounter (prev, _ProgramCounter);

                Raise_ProgramCounter ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_ProgramCounter (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: ProgramCounter (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: SignBit (bool)
        // --------------------------------------------------------------------
        bool _SignBit = default;

        void Raise_SignBit ()
        {
          OnPropertyChanged ("SignBit");
        }

        public bool SignBit
        {
            get { return _SignBit; }
            set
            {
                if (_SignBit == value)
                {
                    return;
                }

                var prev = _SignBit;

                _SignBit = value;

                Changed_SignBit (prev, _SignBit);

                Raise_SignBit ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_SignBit (bool prev, bool current);
        // --------------------------------------------------------------------
        // END_PROPERTY: SignBit (bool)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: ZeroBit (bool)
        // --------------------------------------------------------------------
        bool _ZeroBit = default;

        void Raise_ZeroBit ()
        {
          OnPropertyChanged ("ZeroBit");
        }

        public bool ZeroBit
        {
            get { return _ZeroBit; }
            set
            {
                if (_ZeroBit == value)
                {
                    return;
                }

                var prev = _ZeroBit;

                _ZeroBit = value;

                Changed_ZeroBit (prev, _ZeroBit);

                Raise_ZeroBit ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_ZeroBit (bool prev, bool current);
        // --------------------------------------------------------------------
        // END_PROPERTY: ZeroBit (bool)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: UBit (bool)
        // --------------------------------------------------------------------
        bool _UBit = default;

        void Raise_UBit ()
        {
          OnPropertyChanged ("UBit");
        }

        public bool UBit
        {
            get { return _UBit; }
            set
            {
                if (_UBit == value)
                {
                    return;
                }

                var prev = _UBit;

                _UBit = value;

                Changed_UBit (prev, _UBit);

                Raise_UBit ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_UBit (bool prev, bool current);
        // --------------------------------------------------------------------
        // END_PROPERTY: UBit (bool)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: HalfCarryBit (bool)
        // --------------------------------------------------------------------
        bool _HalfCarryBit = default;

        void Raise_HalfCarryBit ()
        {
          OnPropertyChanged ("HalfCarryBit");
        }

        public bool HalfCarryBit
        {
            get { return _HalfCarryBit; }
            set
            {
                if (_HalfCarryBit == value)
                {
                    return;
                }

                var prev = _HalfCarryBit;

                _HalfCarryBit = value;

                Changed_HalfCarryBit (prev, _HalfCarryBit);

                Raise_HalfCarryBit ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_HalfCarryBit (bool prev, bool current);
        // --------------------------------------------------------------------
        // END_PROPERTY: HalfCarryBit (bool)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: XBit (bool)
        // --------------------------------------------------------------------
        bool _XBit = default;

        void Raise_XBit ()
        {
          OnPropertyChanged ("XBit");
        }

        public bool XBit
        {
            get { return _XBit; }
            set
            {
                if (_XBit == value)
                {
                    return;
                }

                var prev = _XBit;

                _XBit = value;

                Changed_XBit (prev, _XBit);

                Raise_XBit ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_XBit (bool prev, bool current);
        // --------------------------------------------------------------------
        // END_PROPERTY: XBit (bool)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: ParityOverflowBit (bool)
        // --------------------------------------------------------------------
        bool _ParityOverflowBit = default;

        void Raise_ParityOverflowBit ()
        {
          OnPropertyChanged ("ParityOverflowBit");
        }

        public bool ParityOverflowBit
        {
            get { return _ParityOverflowBit; }
            set
            {
                if (_ParityOverflowBit == value)
                {
                    return;
                }

                var prev = _ParityOverflowBit;

                _ParityOverflowBit = value;

                Changed_ParityOverflowBit (prev, _ParityOverflowBit);

                Raise_ParityOverflowBit ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_ParityOverflowBit (bool prev, bool current);
        // --------------------------------------------------------------------
        // END_PROPERTY: ParityOverflowBit (bool)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: NegationBit (bool)
        // --------------------------------------------------------------------
        bool _NegationBit = default;

        void Raise_NegationBit ()
        {
          OnPropertyChanged ("NegationBit");
        }

        public bool NegationBit
        {
            get { return _NegationBit; }
            set
            {
                if (_NegationBit == value)
                {
                    return;
                }

                var prev = _NegationBit;

                _NegationBit = value;

                Changed_NegationBit (prev, _NegationBit);

                Raise_NegationBit ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_NegationBit (bool prev, bool current);
        // --------------------------------------------------------------------
        // END_PROPERTY: NegationBit (bool)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: CarryBit (bool)
        // --------------------------------------------------------------------
        bool _CarryBit = default;

        void Raise_CarryBit ()
        {
          OnPropertyChanged ("CarryBit");
        }

        public bool CarryBit
        {
            get { return _CarryBit; }
            set
            {
                if (_CarryBit == value)
                {
                    return;
                }

                var prev = _CarryBit;

                _CarryBit = value;

                Changed_CarryBit (prev, _CarryBit);

                Raise_CarryBit ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_CarryBit (bool prev, bool current);
        // --------------------------------------------------------------------
        // END_PROPERTY: CarryBit (bool)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: AccuFlags (string)
        // --------------------------------------------------------------------
        string _AccuFlags = default;

        void Raise_AccuFlags ()
        {
          OnPropertyChanged ("AccuFlags");
        }

        public string AccuFlags
        {
            get { return _AccuFlags; }
            set
            {
                if (_AccuFlags == value)
                {
                    return;
                }

                var prev = _AccuFlags;

                _AccuFlags = value;

                Changed_AccuFlags (prev, _AccuFlags);

                Raise_AccuFlags ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_AccuFlags (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: AccuFlags (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: AccuFlagsPrime (string)
        // --------------------------------------------------------------------
        string _AccuFlagsPrime = default;

        void Raise_AccuFlagsPrime ()
        {
          OnPropertyChanged ("AccuFlagsPrime");
        }

        public string AccuFlagsPrime
        {
            get { return _AccuFlagsPrime; }
            set
            {
                if (_AccuFlagsPrime == value)
                {
                    return;
                }

                var prev = _AccuFlagsPrime;

                _AccuFlagsPrime = value;

                Changed_AccuFlagsPrime (prev, _AccuFlagsPrime);

                Raise_AccuFlagsPrime ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_AccuFlagsPrime (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: AccuFlagsPrime (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: HLPair (string)
        // --------------------------------------------------------------------
        string _HLPair = default;

        void Raise_HLPair ()
        {
          OnPropertyChanged ("HLPair");
        }

        public string HLPair
        {
            get { return _HLPair; }
            set
            {
                if (_HLPair == value)
                {
                    return;
                }

                var prev = _HLPair;

                _HLPair = value;

                Changed_HLPair (prev, _HLPair);

                Raise_HLPair ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_HLPair (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: HLPair (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: HLPairPrime (string)
        // --------------------------------------------------------------------
        string _HLPairPrime = default;

        void Raise_HLPairPrime ()
        {
          OnPropertyChanged ("HLPairPrime");
        }

        public string HLPairPrime
        {
            get { return _HLPairPrime; }
            set
            {
                if (_HLPairPrime == value)
                {
                    return;
                }

                var prev = _HLPairPrime;

                _HLPairPrime = value;

                Changed_HLPairPrime (prev, _HLPairPrime);

                Raise_HLPairPrime ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_HLPairPrime (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: HLPairPrime (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: BCPair (string)
        // --------------------------------------------------------------------
        string _BCPair = default;

        void Raise_BCPair ()
        {
          OnPropertyChanged ("BCPair");
        }

        public string BCPair
        {
            get { return _BCPair; }
            set
            {
                if (_BCPair == value)
                {
                    return;
                }

                var prev = _BCPair;

                _BCPair = value;

                Changed_BCPair (prev, _BCPair);

                Raise_BCPair ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_BCPair (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: BCPair (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: BCPairPrime (string)
        // --------------------------------------------------------------------
        string _BCPairPrime = default;

        void Raise_BCPairPrime ()
        {
          OnPropertyChanged ("BCPairPrime");
        }

        public string BCPairPrime
        {
            get { return _BCPairPrime; }
            set
            {
                if (_BCPairPrime == value)
                {
                    return;
                }

                var prev = _BCPairPrime;

                _BCPairPrime = value;

                Changed_BCPairPrime (prev, _BCPairPrime);

                Raise_BCPairPrime ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_BCPairPrime (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: BCPairPrime (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: DEPair (string)
        // --------------------------------------------------------------------
        string _DEPair = default;

        void Raise_DEPair ()
        {
          OnPropertyChanged ("DEPair");
        }

        public string DEPair
        {
            get { return _DEPair; }
            set
            {
                if (_DEPair == value)
                {
                    return;
                }

                var prev = _DEPair;

                _DEPair = value;

                Changed_DEPair (prev, _DEPair);

                Raise_DEPair ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_DEPair (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: DEPair (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: DEPairPrime (string)
        // --------------------------------------------------------------------
        string _DEPairPrime = default;

        void Raise_DEPairPrime ()
        {
          OnPropertyChanged ("DEPairPrime");
        }

        public string DEPairPrime
        {
            get { return _DEPairPrime; }
            set
            {
                if (_DEPairPrime == value)
                {
                    return;
                }

                var prev = _DEPairPrime;

                _DEPairPrime = value;

                Changed_DEPairPrime (prev, _DEPairPrime);

                Raise_DEPairPrime ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_DEPairPrime (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: DEPairPrime (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: StackPointer (string)
        // --------------------------------------------------------------------
        string _StackPointer = default;

        void Raise_StackPointer ()
        {
          OnPropertyChanged ("StackPointer");
        }

        public string StackPointer
        {
            get { return _StackPointer; }
            set
            {
                if (_StackPointer == value)
                {
                    return;
                }

                var prev = _StackPointer;

                _StackPointer = value;

                Changed_StackPointer (prev, _StackPointer);

                Raise_StackPointer ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_StackPointer (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: StackPointer (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: IndexX (string)
        // --------------------------------------------------------------------
        string _IndexX = default;

        void Raise_IndexX ()
        {
          OnPropertyChanged ("IndexX");
        }

        public string IndexX
        {
            get { return _IndexX; }
            set
            {
                if (_IndexX == value)
                {
                    return;
                }

                var prev = _IndexX;

                _IndexX = value;

                Changed_IndexX (prev, _IndexX);

                Raise_IndexX ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_IndexX (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: IndexX (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: IndexY (string)
        // --------------------------------------------------------------------
        string _IndexY = default;

        void Raise_IndexY ()
        {
          OnPropertyChanged ("IndexY");
        }

        public string IndexY
        {
            get { return _IndexY; }
            set
            {
                if (_IndexY == value)
                {
                    return;
                }

                var prev = _IndexY;

                _IndexY = value;

                Changed_IndexY (prev, _IndexY);

                Raise_IndexY ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_IndexY (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: IndexY (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: InterruptVector (string)
        // --------------------------------------------------------------------
        string _InterruptVector = default;

        void Raise_InterruptVector ()
        {
          OnPropertyChanged ("InterruptVector");
        }

        public string InterruptVector
        {
            get { return _InterruptVector; }
            set
            {
                if (_InterruptVector == value)
                {
                    return;
                }

                var prev = _InterruptVector;

                _InterruptVector = value;

                Changed_InterruptVector (prev, _InterruptVector);

                Raise_InterruptVector ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_InterruptVector (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: InterruptVector (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: Mode (int)
        // --------------------------------------------------------------------
        int _Mode = default;

        void Raise_Mode ()
        {
          OnPropertyChanged ("Mode");
        }

        public int Mode
        {
            get { return _Mode; }
            set
            {
                if (_Mode == value)
                {
                    return;
                }

                var prev = _Mode;

                _Mode = value;

                Changed_Mode (prev, _Mode);

                Raise_Mode ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_Mode (int prev, int current);
        // --------------------------------------------------------------------
        // END_PROPERTY: Mode (int)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: Refresh (string)
        // --------------------------------------------------------------------
        string _Refresh = default;

        void Raise_Refresh ()
        {
          OnPropertyChanged ("Refresh");
        }

        public string Refresh
        {
            get { return _Refresh; }
            set
            {
                if (_Refresh == value)
                {
                    return;
                }

                var prev = _Refresh;

                _Refresh = value;

                Changed_Refresh (prev, _Refresh);

                Raise_Refresh ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_Refresh (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: Refresh (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: Memory (Dictionary<string,string>)
        // --------------------------------------------------------------------
        Dictionary<string,string> _Memory = default;

        void Raise_Memory ()
        {
          OnPropertyChanged ("Memory");
        }

        public Dictionary<string,string> Memory
        {
            get { return _Memory; }
            set
            {
                if (_Memory == value)
                {
                    return;
                }

                var prev = _Memory;

                _Memory = value;

                Changed_Memory (prev, _Memory);

                Raise_Memory ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_Memory (Dictionary<string,string> prev, Dictionary<string,string> current);
        // --------------------------------------------------------------------
        // END_PROPERTY: Memory (Dictionary<string,string>)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: MemoryMapRow (int)
        // --------------------------------------------------------------------
        int _MemoryMapRow = default;

        void Raise_MemoryMapRow ()
        {
          OnPropertyChanged ("MemoryMapRow");
        }

        public int MemoryMapRow
        {
            get { return _MemoryMapRow; }
            set
            {
                if (_MemoryMapRow == value)
                {
                    return;
                }

                var prev = _MemoryMapRow;

                _MemoryMapRow = value;

                Changed_MemoryMapRow (prev, _MemoryMapRow);

                Raise_MemoryMapRow ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_MemoryMapRow (int prev, int current);
        // --------------------------------------------------------------------
        // END_PROPERTY: MemoryMapRow (int)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: DisAsm (Dictionary<string,string>)
        // --------------------------------------------------------------------
        Dictionary<string,string> _DisAsm = default;

        void Raise_DisAsm ()
        {
          OnPropertyChanged ("DisAsm");
        }

        public Dictionary<string,string> DisAsm
        {
            get { return _DisAsm; }
            set
            {
                if (_DisAsm == value)
                {
                    return;
                }

                var prev = _DisAsm;

                _DisAsm = value;

                Changed_DisAsm (prev, _DisAsm);

                Raise_DisAsm ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_DisAsm (Dictionary<string,string> prev, Dictionary<string,string> current);
        // --------------------------------------------------------------------
        // END_PROPERTY: DisAsm (Dictionary<string,string>)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: DisassmFrom (string)
        // --------------------------------------------------------------------
        string _DisassmFrom = default;

        void Raise_DisassmFrom ()
        {
          OnPropertyChanged ("DisassmFrom");
        }

        public string DisassmFrom
        {
            get { return _DisassmFrom; }
            set
            {
                if (_DisassmFrom == value)
                {
                    return;
                }

                var prev = _DisassmFrom;

                _DisassmFrom = value;

                Changed_DisassmFrom (prev, _DisassmFrom);

                Raise_DisassmFrom ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_DisassmFrom (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: DisassmFrom (string)
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_PROPERTY: DisassmTo (string)
        // --------------------------------------------------------------------
        string _DisassmTo = default;

        void Raise_DisassmTo ()
        {
          OnPropertyChanged ("DisassmTo");
        }

        public string DisassmTo
        {
            get { return _DisassmTo; }
            set
            {
                if (_DisassmTo == value)
                {
                    return;
                }

                var prev = _DisassmTo;

                _DisassmTo = value;

                Changed_DisassmTo (prev, _DisassmTo);

                Raise_DisassmTo ();
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_DisassmTo (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: DisassmTo (string)
        // --------------------------------------------------------------------


        // --------------------------------------------------------------------
        // BEGIN_COMMAND: StepCommand
        // --------------------------------------------------------------------
        readonly UserCommand _StepCommand;

        bool CanExecuteStepCommand ()
        {
          bool result = false;
          CanExecute_StepCommand (ref result);

          return result;
        }

        void ExecuteStepCommand ()
        {
          Execute_StepCommand ();
        }

        public ICommand StepCommand { get { return _StepCommand;} }
        // --------------------------------------------------------------------
        partial void CanExecute_StepCommand (ref bool result);
        partial void Execute_StepCommand ();
        // --------------------------------------------------------------------
        // END_COMMAND: StepCommand
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_COMMAND: LoadCommand
        // --------------------------------------------------------------------
        readonly UserCommand _LoadCommand;

        bool CanExecuteLoadCommand ()
        {
          bool result = false;
          CanExecute_LoadCommand (ref result);

          return result;
        }

        void ExecuteLoadCommand ()
        {
          Execute_LoadCommand ();
        }

        public ICommand LoadCommand { get { return _LoadCommand;} }
        // --------------------------------------------------------------------
        partial void CanExecute_LoadCommand (ref bool result);
        partial void Execute_LoadCommand ();
        // --------------------------------------------------------------------
        // END_COMMAND: LoadCommand
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // BEGIN_COMMAND: DisassembleCommand
        // --------------------------------------------------------------------
        readonly UserCommand _DisassembleCommand;

        bool CanExecuteDisassembleCommand ()
        {
          bool result = false;
          CanExecute_DisassembleCommand (ref result);

          return result;
        }

        void ExecuteDisassembleCommand ()
        {
          Execute_DisassembleCommand ();
        }

        public ICommand DisassembleCommand { get { return _DisassembleCommand;} }
        // --------------------------------------------------------------------
        partial void CanExecute_DisassembleCommand (ref bool result);
        partial void Execute_DisassembleCommand ();
        // --------------------------------------------------------------------
        // END_COMMAND: DisassembleCommand
        // --------------------------------------------------------------------


        partial void Constructed ();

        public MainWindowViewModel (Dispatcher dispatcher)
        {
          _dispatcher = dispatcher;
          _StepCommand = new UserCommand (CanExecuteStepCommand, ExecuteStepCommand);
          _LoadCommand = new UserCommand (CanExecuteLoadCommand, ExecuteLoadCommand);
          _DisassembleCommand = new UserCommand (CanExecuteDisassembleCommand, ExecuteDisassembleCommand);

          Constructed ();
        }

        void ResetCanExecute ()
        {
          _StepCommand.RefreshCanExecute ();
          _LoadCommand.RefreshCanExecute ();
          _DisassembleCommand.RefreshCanExecute ();
        }

        void Dispatch(Action action)
        {
          _dispatcher.BeginInvoke(action);
        }

        protected virtual void OnPropertyChanged (string propertyChanged)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyChanged));
        }
    }
}

