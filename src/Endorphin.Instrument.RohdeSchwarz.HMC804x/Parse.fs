// Copyright (c) University of Warwick. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

namespace Endorphin.Instrument.RohdeSchwarz.HMC804x

open Endorphin.Core
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

/// Functions to translate between internal and machine representations.
module Parse =
    /// Convert a machine representation of an on/off state into an internal representation.
    let onOffState str =
        match String.toUpper str with
        | "0" | "OFF" -> Off
        | "1" | "ON"  -> On
        | str         -> raise << UnexpectedReplyException <| sprintf "Unexpected on-off string: %s." str

    let output str =
        match String.toUpper str with
        | "OUT1" | "OUTPUT1" -> OUT1
        | "OUT2" | "OUTPUT2" -> OUT1
        | "OUT3" | "OUTPUT3" -> OUT1
        | str -> raise << UnexpectedReplyException <| sprintf "Unexpected output channel string: %s." str

    let interpolation = function
        | "0" -> Step
        | "1" -> Interpolate
        | str -> raise << UnexpectedReplyException <| sprintf "Unexpected interpolation: %s." str

    let current (str:string) =
        Current_A (float str * 1.0<A>)

    let voltage (str:string) =
        Voltage_V (float str * 1.0<V>)

    let time str =
        Time_s (float str * 1.0<s>)

    let power str =
        Power_W (float str * 1.0<W>)

    let arbRepetitions = function
        | Repetitions n -> n
        | Forever -> 0us

    let arbTriggerMode str =
        match String.toUpper str with
        | "SING"
        | "SINGLE"  -> Single
        | "RUN"     -> Run
        | str       -> raise << UnexpectedReplyException <| sprintf "Unexpected trigger mode string: %s." str