// Copyright (c) University of Warwick. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

namespace Endorphin.Instrument.RohdeSchwarz.HMC804x

open Endorphin.Core

module IO =
    /// Set a key to a certain value in the instrument.
    let set<'In> key (value : 'In) (HMC804x instrument) = SCPI.Checked.Set.value key value instrument
    /// Post a key to the instrument.
    let post key (HMC804x instrument) = SCPI.Checked.Set.key key instrument
    /// Query the instrument for a value.
    let query parser key (HMC804x instrument) = SCPI.Checked.Query.Key.parsed parser key instrument

    /// Extract the SCPI instrument from the RfSource handle
    let scpiInstrument (HMC804x instrument) = instrument

    /// Check that the identity of an instrument is one we're able to control.
    let private checkIdentity (HMC804x instrument) = async {
        let! identity = SCPI.Query.identity instrument
        match identity.Model with
        | "HMC8043" -> ()
        | _         ->
            sprintf "Unknown model number: %s" identity.Model
            |> UnexpectedReplyException
            |> raise }

    /// Perform the initialisation checks, and raise an exception if any fail.
    let initialisationChecks instrument = async {
        do! checkIdentity instrument
        do! SCPI.Checked.Query.errors (scpiInstrument instrument) }

    /// Reset the instrument
    let reset = fun x -> post "*RST" x

    /// Mark the instrument as under remote operation
    let remote = fun x -> post ":SYSTEM:REMOTE" x

    /// Mark the instrument as under local operation
    let local = fun x -> post ":SYSTEM:LOCAL" x

    /// Connect to an instrument, and mark it as under remote operation.
    let connect visaTcpipAddress timeout = async {
        let instrument = Visa.openTcpipInstrument visaTcpipAddress timeout None :> SCPI.IScpiInstrument |> HMC804x
        do! reset instrument
        do! remote instrument
        do! initialisationChecks instrument
        return instrument }

    /// Disconnect from an instrument, passing control back locally.
    let disconnect instrument = async {
        do! local instrument
        do! Visa.closeInstrument (scpiInstrument instrument :?> Visa.Instrument) }
