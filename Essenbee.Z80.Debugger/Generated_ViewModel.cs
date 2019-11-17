using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        // BEGIN_PROPERTY: Memory (string)
        // --------------------------------------------------------------------
        string _Memory = default;

        void Raise_Memory ()
        {
          OnPropertyChanged ("Memory");
        }

        public string Memory
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
        partial void Changed_Memory (string prev, string current);
        // --------------------------------------------------------------------
        // END_PROPERTY: Memory (string)
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


        partial void Constructed ();

        public MainWindowViewModel (Dispatcher dispatcher)
        {
          _dispatcher = dispatcher;
          _StepCommand = new UserCommand (CanExecuteStepCommand, ExecuteStepCommand);
          _LoadCommand = new UserCommand (CanExecuteLoadCommand, ExecuteLoadCommand);

          Constructed ();
        }

        void ResetCanExecute ()
        {
          _StepCommand.RefreshCanExecute ();
          _LoadCommand.RefreshCanExecute ();
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

