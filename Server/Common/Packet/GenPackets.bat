START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
XCOPY /Y GenPackets.cs "../../Server/Packet"
XCOPY /Y ClinentPacketManager.cs "../../DummyClient/Packet"
XCOPY /Y ServerPacketManager.cs "../../Server/Packet"