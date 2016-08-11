// Copyright (c) University of Warwick. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

#I "../../packages/"

#r "Endorphin.Core/lib/net452/Endorphin.Core.dll"
#r "Endorphin.Core.NationalInstruments/lib/net452/Endorphin.Core.NationalInstruments.dll"
#r "log4net/lib/net45-full/log4net.dll"
#r "bin/Debug/Endorphin.Instrument.RohdeSchwarz.HMC804x.dll"

open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Endorphin.Instrument.RohdeSchwarz.HMC804x
open Endorphin.Core
open HMC804x

log4net.Config.BasicConfigurator.Configure ()


let settings : PowerSupplySettings = [ (OUT1, constantOutput 5.0<V> 0.2<A> )
                                       (OUT2, rampContinuous 0.0<V> 0.1<A> 2.0<V> 0.1<A> 1.0<s> 0.2<s> |> withMaximumPower 0.5<W>)
                                       (OUT3, rampContinuous 1.0<V> 0.1<A> 4.0<V> 0.1<A> 1.0<s> 0.2<s> |> withMaximumPower 0.5<W>) ]
                                     |> Map.ofList

try
    async {
        // open the Rohde & Schwartz current source - set the VISA access string you need here and timeout
        let! source = IO.connect "TCPIP0::10.0.0.2::5025::SOCKET" 5000<ms>
        do! applySettings settings source
        do! start source
        do! Async.Sleep 5000
        // tidy up and close
        do! IO.disconnect source }
    |> Async.RunSynchronously
with
    | :? SCPI.InstrumentErrorException as exn -> printfn "Failed with instrument errors: %A" exn.Data0
