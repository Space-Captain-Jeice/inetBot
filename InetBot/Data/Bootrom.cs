namespace InetBot.Data.Bootrom
{
    public enum BootErrorSleepSwitch : byte
    {
        SleepSwitchOpen = 0,
        SleepSwitchGPIOClosed = 1,
        SleepSwitchMCUClosed = 2
    };

    public enum BootStatusCodes : byte
    {
        NotProcessed = 0x00,
        SkippedNonFirmware = 0xFF,
        DeviceInitializationFailed = 0xFE,
        SDDriverInitFailedDueToInvalidBoot9InitState = 0xFD,
        FIRMHeaderInvalidMagic = 0xF8,
        FIRMLoadingSkippedDueToEqualOrHigherPriorityFIRMToLoad = 0xF7,
        FailureToLoadNCSDHeaderFromNAND = 0xEF,
        NCSDHeaderInvalidMagicOrInvalidSignature = 0xEE,
        FailureToLoadFIRMHeaderFromDevice = 0xDF,
        FIRMHeaderInvalidMagicOrInvalidSignature = 0xDE,
        FirmwareSectionLoadingTriggeredAddressBlacklistFailedToLoadSectionToMemoryOrFIRMSectionHashInvalid = 0xCF
    };

    public enum SDDriverErrorBits : uint
    {
        Status2CommandFieldMismatchedSent = 1u << 0,
        Status2CommandCRCMismatched = 1u << 1,
        Status2FramingErrorNoStopBit = 1u << 2,
        Status2DataTimeout = 1u << 3,
        Status2RXFIFOOverflow = 1u << 4,
        Status2TXFIFOOverflow = 1u << 5,
        Status2Bit31IllegalAccessError = 1u << 6,
        ErrorBitsBySDDeviceOrUnexpectedState = 1u << 7,
        IllegalCommandBitReceived = 1u << 8,
        TimerBasedSDOperationTimeout = 1u << 9,
        TimerBasedMMCInitSequenceTimeout = 1u << 10,
        UnknownKindOfTimeout = 1u << 11,
        AESOperationOnSectorTimeout = 1u << 15,
        AESBusyError = 1u << 19,

        UnknownBits = 0xFFF77000u
    };

    // if the errors are for NAND access, it gets AND-ed with 0xFDFF0080, otherwise with SD it gets AND-ed with 0xFDF90008
    // So I'll consider only the required bits 0xFDFF0088
    public enum SDHardwareErrorBits : uint
    {
        AuthenticationSequenceError = 1u << 3, // AKE_SEQ_ERROR
        Reserved7 = 1u << 7,
        CSDOverwrite = 1u << 16, // CSD_OVERWRITE
        Reserved17 = 1u << 17,
        Reserved18 = 1u << 18,
        GeneralOrUnknownError = 1u << 19, // ERROR
        CardControllerError = 1u << 20, // CC_ERROR
        CardECCFailure = 1u << 21, // CARD_ECC_FAILED
        IllegalCommand = 1u << 22, // ILLEGAL_COMMAND
        CommandCRCError = 1u << 23, // COM_CRC_ERROR
        LockUnloadFailure = 1u << 24, // LOCK_UNLOCK_FAILED
        WriteProtectionViolation = 1u << 26, // WP_VIOLATION
        EraseParameterInvalid = 1u << 27, // ERASE_PARAM
        EraseSequenceError = 1u << 28, // ERASE_SEQ_ERROR
        BlockLengthError = 1u << 29, // BLOCK_LEN_ERROR
        AddressError = 1u << 30, // ADDRESS_ERROR
        OutOfRange = 1u << 31, // OUT_OF_RANGE

        UnobtainableBits = 0x0200FF77u // as per bootrom errors
    };

    public class BootError {
        public BootErrorSleepSwitch SleepSwitch = 0;
        public BootStatusCodes NVRAMLoadCode = BootStatusCodes.NotProcessed; // Wifi's NVRAM can have loadable firmware
        public BootStatusCodes NTRBootLoadCode = BootStatusCodes.NotProcessed;
        public BootStatusCodes NANDLoadCode = BootStatusCodes.NotProcessed;
        public BootStatusCodes[] FirmPartitionLoadCodes = new BootStatusCodes[8];
        public uint SDDriverError = 0;
        public uint SDHardwareError = 0;

        public BootError(uint firstcode, uint firmpartitioncodes1 = 0, uint firmpartitioncodes2 = 0, uint sddrivererror = 0, uint sdhardwareerror = 0)
        {
            SleepSwitch = (BootErrorSleepSwitch)((firstcode >> 24) & 0xFF);
            NVRAMLoadCode = (BootStatusCodes)((firstcode >> 16) & 0xFF);
            NTRBootLoadCode = (BootStatusCodes)((firstcode >> 8) & 0xFF);
            NANDLoadCode = (BootStatusCodes)(firstcode & 0xFF);
            FirmPartitionLoadCodes[0] = (BootStatusCodes)(firmpartitioncodes1 & 0xFF);
            FirmPartitionLoadCodes[1] = (BootStatusCodes)((firmpartitioncodes1 >> 8) & 0xFF);
            FirmPartitionLoadCodes[2] = (BootStatusCodes)((firmpartitioncodes1 >> 16) & 0xFF);
            FirmPartitionLoadCodes[3] = (BootStatusCodes)((firmpartitioncodes1 >> 24) & 0xFF);
            FirmPartitionLoadCodes[4] = (BootStatusCodes)(firmpartitioncodes2 & 0xFF);
            FirmPartitionLoadCodes[5] = (BootStatusCodes)((firmpartitioncodes2 >> 8) & 0xFF);
            FirmPartitionLoadCodes[6] = (BootStatusCodes)((firmpartitioncodes2 >> 16) & 0xFF);
            FirmPartitionLoadCodes[7] = (BootStatusCodes)((firmpartitioncodes2 >> 24) & 0xFF);
            SDDriverError = sddrivererror;
            SDHardwareError = sdhardwareerror;
        }

        public static string TranslateSleepSwitch(BootErrorSleepSwitch SleepSwitch)
        {
            return SleepSwitch switch
            {
                BootErrorSleepSwitch.SleepSwitchOpen => "Sleep Switch Open",
                BootErrorSleepSwitch.SleepSwitchGPIOClosed => "GPIO Sleep Switch Closed",
                BootErrorSleepSwitch.SleepSwitchMCUClosed => "MCU Sleep Switch Closed",
                _ => $"Sleep Switch 0x{(byte)SleepSwitch:X2} is unknown"
            };
        }

        public static string TranslateBootStatusCode(BootStatusCodes StatusCode)
        {
            return StatusCode switch
            {
                BootStatusCodes.NotProcessed => "Device was not considered to be loaded",
                BootStatusCodes.SkippedNonFirmware => "Partition skipped due to it not being a FIRM partition",
                BootStatusCodes.DeviceInitializationFailed => "Device initialization failed due to it missing or malfunctioning",
                BootStatusCodes.SDDriverInitFailedDueToInvalidBoot9InitState => "SD driver initialization failed due to boot9 state not being initialized correctly",
                BootStatusCodes.FIRMHeaderInvalidMagic => "The FIRM header magic is not matching \"FIRM\"",
                BootStatusCodes.FIRMLoadingSkippedDueToEqualOrHigherPriorityFIRMToLoad => "FIRM image loading got skipped due to already having found an equal or higher priority (firmhdr+4) FIRM to load",
                BootStatusCodes.FailureToLoadNCSDHeaderFromNAND => "Failed to load NCSD header from NAND",
                BootStatusCodes.NCSDHeaderInvalidMagicOrInvalidSignature => "NCSD header magic is not \"NCSD\", or NCSD header RSA verification failed",
                BootStatusCodes.FailureToLoadFIRMHeaderFromDevice => "Failed to read FIRM header from device",
                BootStatusCodes.FIRMHeaderInvalidMagicOrInvalidSignature => "FIRM header magic is not \"FIRM\", or FIRM header RSA verification failed",
                BootStatusCodes.FirmwareSectionLoadingTriggeredAddressBlacklistFailedToLoadSectionToMemoryOrFIRMSectionHashInvalid => "FIRM section loading failed for any of these reasons: FIRM section load address blacklist got tripped; Failed to read FIRM section data into memory; Or, FIRM section hash verification failed",
                _ => $"Status Code 0x{(byte)StatusCode:X2} is unknown"
            };
        }

        public static string[] TranslateSDDriverError(uint SDDriverError)
        {
            uint unknownbits = SDDriverError & (uint)SDDriverErrorBits.UnknownBits;
            List<string> messages = new();
            if ((SDDriverError & (uint)SDDriverErrorBits.Status2CommandFieldMismatchedSent) != 0) messages.Add("STATUS2: received cmd field does not match what was sent");
            if ((SDDriverError & (uint)SDDriverErrorBits.Status2CommandCRCMismatched) != 0) messages.Add("STATUS2: received CRC does not match what was calculated");
            if ((SDDriverError & (uint)SDDriverErrorBits.Status2FramingErrorNoStopBit) != 0) messages.Add("STATUS2: framing error, stop bit was not encountered");
            if ((SDDriverError & (uint)SDDriverErrorBits.Status2DataTimeout) != 0) messages.Add("STATUS2: data was not received within the timeout period");
            if ((SDDriverError & (uint)SDDriverErrorBits.Status2RXFIFOOverflow) != 0) messages.Add("STATUS2: RX FIFO overflow");
            if ((SDDriverError & (uint)SDDriverErrorBits.Status2TXFIFOOverflow) != 0) messages.Add("STATUS2: TX FIFO overflow");
            if ((SDDriverError & (uint)SDDriverErrorBits.Status2Bit31IllegalAccessError) != 0) messages.Add("STATUS2 (bit31): illegal access error (???)");
            if ((SDDriverError & (uint)SDDriverErrorBits.ErrorBitsBySDDeviceOrUnexpectedState) != 0) messages.Add("At least one error bit was set in the command reply from the SD device, or other unexpected state is reported");
            if ((SDDriverError & (uint)SDDriverErrorBits.IllegalCommandBitReceived) != 0) messages.Add("An illegal command was received by the SD device (ILLEGAL_COMMAND bit set)");
            if ((SDDriverError & (uint)SDDriverErrorBits.TimerBasedSDOperationTimeout) != 0) messages.Add("Timer-based timeout while waiting for SD device operations to finish");
            if ((SDDriverError & (uint)SDDriverErrorBits.TimerBasedMMCInitSequenceTimeout) != 0) messages.Add("Got a timer-based timeout during MMC initialization sequence");
            if ((SDDriverError & (uint)SDDriverErrorBits.UnknownKindOfTimeout) != 0) messages.Add("??? some sort of timeout");
            if ((SDDriverError & (uint)SDDriverErrorBits.AESOperationOnSectorTimeout) != 0) messages.Add("Timeout while trying to perform AES operation on sector data");
            if ((SDDriverError & (uint)SDDriverErrorBits.AESBusyError) != 0) messages.Add("Tried to perform AES operation while another AES operation is taking place");
            if (unknownbits != 0) messages.Add($"Unknown SD/MMC Driver Error Bits: 0x{unknownbits:X8}");
            return messages.ToArray();
        }

        public static string[] TranslateSDHardwareError(uint SDHardwareError)
        {
            uint unknownbits = SDHardwareError & (uint)SDHardwareErrorBits.UnobtainableBits;
            List<string> messages = new();
            if ((SDHardwareError & (uint)SDHardwareErrorBits.AuthenticationSequenceError) != 0) messages.Add("Error in the sequence of the authentication");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.Reserved7) != 0) messages.Add("Reserved7");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.CSDOverwrite) != 0) messages.Add("CSD Overwrite: Read only section of CSD does not match the card content; Or, attempt to reverse the copy or permanent WP bits was made");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.Reserved17) != 0) messages.Add("Reserved17");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.Reserved18) != 0) messages.Add("Reserved18");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.GeneralOrUnknownError) != 0) messages.Add("General/unknown error occurred during the operation");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.CardControllerError) != 0) messages.Add("Internal card controller error");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.CardECCFailure) != 0) messages.Add("Internal ECC applied but failed to correct the data");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.IllegalCommand) != 0) messages.Add("Illegal command for card state");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.CommandCRCError) != 0) messages.Add("CRC check of the previous command failed");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.LockUnloadFailure) != 0) messages.Add("Sequence/password error detected in a lock/unlock command");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.WriteProtectionViolation) != 0) messages.Add("Host attempted to write to a protected block or to the temporary or permanent write protected card");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.EraseParameterInvalid) != 0) messages.Add("An invalid selection of write-blocks for erase occurred");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.EraseSequenceError) != 0) messages.Add("An error in the sequence of erase commands occurred");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.BlockLengthError) != 0) messages.Add("The transferred block length is not allowed for this card, or the number of transferred bytes does not match the block length");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.AddressError) != 0) messages.Add("Misaligned address which did not match the block length in the command");
            if ((SDHardwareError & (uint)SDHardwareErrorBits.OutOfRange) != 0) messages.Add("Command's argument was out of the allowed range for this card");
            if (unknownbits != 0) messages.Add($"Unobtainable SD/EMMC Status Bits: 0x{unknownbits:X8}");
            return messages.ToArray();
        }
    };
}
