using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



// ############################################################################
// #                                                                          #
// #        ---==>  T H I S  F I L E  I S   G E N E R A T E D  <==---         #
// #                                                                          #
// # This means that any edits to the .cs file will be lost when its          #
// # regenerated. Changes should instead be applied to the corresponding      #
// # template file (.tt)                                                      #
// ############################################################################






// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable InconsistentNaming
// ReSharper disable InvocationIsSkipped
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable RedundantAssignment
// ReSharper disable RedundantCast
// ReSharper disable RedundantUsingDirective
// ReSharper disable UnusedMember.Local

namespace Essenbee.Z80.Debugger
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    using System.Windows;
    using System.Windows.Media;

    // ------------------------------------------------------------------------
    // MsxScreen
    // ------------------------------------------------------------------------
    partial class MsxScreen
    {
        #region Uninteresting generated code
        public static readonly DependencyProperty ScreenProperty = DependencyProperty.Register (
            "Screen",
            typeof (int),
            typeof (MsxScreen),
            new FrameworkPropertyMetadata (
                0x4000,
                FrameworkPropertyMetadataOptions.AffectsRender,
                Changed_Screen,
                Coerce_Screen
            ));

        static void Changed_Screen (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var instance = dependencyObject as MsxScreen;
            if (instance != null)
            {
                var oldValue = (int)eventArgs.OldValue;
                var newValue = (int)eventArgs.NewValue;

                instance.Changed_Screen (oldValue, newValue);
            }
        }


        static object Coerce_Screen (DependencyObject dependencyObject, object basevalue)
        {
            var instance = dependencyObject as MsxScreen;
            if (instance == null)
            {
                return basevalue;
            }
            var value = (int)basevalue;

            instance.Coerce_Screen (ref value);


            return value;
        }

        public static readonly DependencyProperty RawMemoryProperty = DependencyProperty.Register (
            "RawMemory",
            typeof (IReadOnlyCollection<byte>),
            typeof (MsxScreen),
            new FrameworkPropertyMetadata (
                null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                Changed_RawMemory,
                Coerce_RawMemory
            ));

        static void Changed_RawMemory (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var instance = dependencyObject as MsxScreen;
            if (instance != null)
            {
                var oldValue = (IReadOnlyCollection<byte>)eventArgs.OldValue;
                var newValue = (IReadOnlyCollection<byte>)eventArgs.NewValue;

                instance.Changed_RawMemory (oldValue, newValue);
            }
        }


        static object Coerce_RawMemory (DependencyObject dependencyObject, object basevalue)
        {
            var instance = dependencyObject as MsxScreen;
            if (instance == null)
            {
                return basevalue;
            }
            var value = (IReadOnlyCollection<byte>)basevalue;

            instance.Coerce_RawMemory (ref value);


            return value;
        }

        #endregion

        // --------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------
        public MsxScreen ()
        {
            CoerceAllProperties ();
            Constructed__MsxScreen ();
        }
        // --------------------------------------------------------------------
        partial void Constructed__MsxScreen ();
        // --------------------------------------------------------------------
        void CoerceAllProperties ()
        {
            CoerceValue (ScreenProperty);
            CoerceValue (RawMemoryProperty);
        }


        // --------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------


        // --------------------------------------------------------------------
        public int Screen
        {
            get
            {
                return (int)GetValue (ScreenProperty);
            }
            set
            {
                if (Screen != value)
                {
                    SetValue (ScreenProperty, value);
                }
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_Screen (int oldValue, int newValue);
        partial void Coerce_Screen (ref int coercedValue);
        // --------------------------------------------------------------------



        // --------------------------------------------------------------------
        public IReadOnlyCollection<byte> RawMemory
        {
            get
            {
                return (IReadOnlyCollection<byte>)GetValue (RawMemoryProperty);
            }
            set
            {
                if (RawMemory != value)
                {
                    SetValue (RawMemoryProperty, value);
                }
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_RawMemory (IReadOnlyCollection<byte> oldValue, IReadOnlyCollection<byte> newValue);
        partial void Coerce_RawMemory (ref IReadOnlyCollection<byte> coercedValue);
        // --------------------------------------------------------------------


    }
    // ------------------------------------------------------------------------

}
