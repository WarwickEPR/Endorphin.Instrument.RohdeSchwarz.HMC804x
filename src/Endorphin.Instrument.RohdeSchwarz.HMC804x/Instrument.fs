// Copyright (c) University of Warwick. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

namespace Endorphin.Instrument.RohdeSchwarz.HMC804x

/// Command set of the Keysight RF instrument.
/// Implements functions to modify & query configuration.
/// Organised by subsystem mirroring the Keysight configuration.
[<AutoOpen>]
module Instrument =
    /// Selectable outputs of device
    let availableOutputs = function
        | HMC8041 -> [ OUT1 ] |> Set.ofList
        | HMC8042 -> [ OUT1; OUT2 ] |> Set.ofList
        | HMC8043 -> [ OUT1; OUT2; OUT3 ] |> Set.ofList

    /// Select an output channel to apply subsequent configuration to
    let selectOutput = fun x -> IO.set<Output> ":INST" x
    /// Set the current in the machine to a given value.
    let setCurrent = fun x -> IO.set<Current> ":CURR" x

    /// Set the voltage of the machine to a given value.
    let setVoltage = fun x -> IO.set<Voltage> ":VOLT" x

    /// Enable/Disable the currently selected channel
    let setChannelOutput = fun x -> IO.set<OnOffState> ":OUTPUT:CHANNEL" x

    /// Enable/Disable all enabled channels
    let setMasterOutput = fun x -> IO.set<OnOffState> ":OUTPUT:MASTER" x

    /// Set the sequence of points for the ARB to go through.
    let setArbData = fun x -> IO.set<ArbSequence> ":ARB:DATA" x

    /// Set the ARB triggering mode to on or off.
    let setArbTriggering = fun x -> IO.set<OnOffState> ":ARB:TRIG" x

    /// Set the number of repetitions for the ARB system to the desired number.
    let setArbRepetitions = fun x -> IO.set<ArbRepetitions> ":ARB:REP" x

    /// Set the ARB trigger mode to the desired type.
    let setArbTriggerMode = fun x -> IO.set<ArbTriggerMode> ":ARB:TRIG:MODE" x

    /// Turn the ARB on and off.
    let setArb = fun x -> IO.set<OnOffState> ":ARB" x

    /// Apply output settings to the machine, without setting the maximum power.
    let applyOutputSettings settings instrument = async {
        match settings.Output with
        | Fixed (V,I)  -> do! setCurrent I instrument
                          do! setVoltage V instrument
        | Arb arb -> do! setArbData arb.Sequence instrument
                     do! setArbTriggering arb.Triggering instrument
                     do! setArbRepetitions arb.Repetitions instrument
                     do! setArbTriggerMode arb.TriggerMode instrument
                     do! setArb On instrument }

    /// Apply a set of settings to the given current/voltage source, including
    /// maximum power.
    let applySettings powerSupplySettings instrument = async {
        for KeyValue(output,settings) in powerSupplySettings do
            do! selectOutput output instrument
            do! applyOutputSettings settings instrument
            do! setChannelOutput On instrument
        }

    /// Turn the instrument's master output on.
    let start = fun x -> setMasterOutput On x

    /// Create a settings record to have a constant output.
    let constantOutput voltage current =
        { Output     = Fixed (Voltage_V voltage, Current_A current)
          PowerLimit = None }

    /// Make a single ARB point.
    let makeArbPoint voltage current time interpolation = {
            Voltage       = Voltage_V voltage
            Current       = Current_A current
            Time          = Time_s time
            Interpolation = interpolation }

    /// Make output settings with no power limit.
    let makeArbSettings points reps triggering triggerMode =
        { Sequence = ArbSequence points
          Repetitions = reps
          Triggering = triggering
          TriggerMode = triggerMode } |> Arb

    /// Create a settings record to create ramped output which repeats indefinitely.
    let rampContinuous startVoltage startCurrent finishVoltage finishCurrent riseTime fallTime =
        let start  = makeArbPoint startVoltage startCurrent riseTime Interpolate
        let finish = makeArbPoint finishVoltage finishCurrent fallTime Interpolate
        { Output = makeArbSettings [ start ; finish ] Forever Off Run
          PowerLimit = None }

    /// Create a settings record to create ramped output based on a trigger.
    let rampTriggered startVoltage startCurrent finishVoltage finishCurrent riseTime fallTime =
        let start  = makeArbPoint startVoltage startCurrent riseTime Interpolate
        let finish = makeArbPoint finishVoltage finishCurrent fallTime Interpolate
        { Output = makeArbSettings [ start; finish ] Forever On Single
          PowerLimit = None }

    /// Change the maximum allowed power in a record of settings.
    let withMaximumPower power settings =
        { settings with PowerLimit = Some <| MaximumPower (Power_W power) }

    /// Connect to instrument via TCPIP
    let openInstrument = IO.connect

    /// Disconnect from instrument
    let closeInstrument = IO.disconnect