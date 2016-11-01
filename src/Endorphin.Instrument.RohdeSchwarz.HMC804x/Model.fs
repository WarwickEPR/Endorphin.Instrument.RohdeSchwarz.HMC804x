// Copyright (c) University of Warwick. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

namespace Endorphin.Instrument.RohdeSchwarz.HMC804x

open Endorphin.Core
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

type CurrentSource = internal HMC804x of SCPI.IScpiInstrument
exception UnexpectedReplyException of string

[<AutoOpen>]
module Model =
    type Model = HMC8041 | HMC8042 | HMC8043

    type Output = OUT1 | OUT2 | OUT3 with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () = this |> function
                | OUT1 -> "OUT1"
                | OUT2 -> "OUT2"
                | OUT3 -> "OUT3"

    type Voltage = Voltage_V of float<V> with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () = match this with Voltage_V that -> that |> sprintf "%e"

    type Current = Current_A of float<A> with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () = match this with Current_A that -> that |> sprintf "%e"

    type Power   = Power_W   of float<W> with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () = match this with Power_W that -> that |> sprintf "%e"

    type Time    = Time_s    of float<s> with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () = match this with Time_s that -> that |> sprintf "%e"

    /// A toggle state, where something can either be On or Off.
    type OnOffState = On | Off with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () = this |> function
                | On  -> "ON"
                | Off -> "OFF"

    type Interpolation = Interpolate | Step with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () = this |> function
                | Interpolate -> "1"
                | Step -> "0"

    type ArbPoint = {
        Voltage : Voltage
        Current : Current
        Time    : Time
        Interpolation : Interpolation } with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () =
                [ SCPI.format this.Voltage
                  SCPI.format this.Current
                  SCPI.format this.Time
                  SCPI.format this.Interpolation ]
                |> String.concat ","

    type ArbSequence = ArbSequence of ArbPoint seq with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () =
                match this with ArbSequence that -> that
                |> Seq.map SCPI.format
                |> String.concat ","

    type ArbRepetitions =
    | Repetitions of uint16 // Up to 255
    | Forever with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () = this |> function
                | Repetitions n -> string n
                | Forever       -> "0"

    type ArbTriggerMode = Single | Run with
        interface SCPI.IScpiFormatable with
            member this.ToScpiString () = this |> function
                | Single -> "SING"
                | Run    -> "RUN"

    type ArbSettings = {
        Sequence    : ArbSequence
        Repetitions : ArbRepetitions
        Triggering  : OnOffState
        TriggerMode : ArbTriggerMode }

    type OutputSetting =
    | Fixed of (Voltage * Current)
    | Arb   of ArbSettings

    type PowerLimit = MaximumPower of Power

    type Settings = {
        Output     : OutputSetting
        PowerLimit : PowerLimit option }

    type PowerSupplySettings = Map<Output,Settings>