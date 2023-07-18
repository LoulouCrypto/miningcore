using Miningcore.Extensions;
using Org.BouncyCastle.Math;

namespace Miningcore.Blockchain.Ravencoin;

public static class RavencoinUtils
{
    public static string EncodeTarget(double difficulty)
    {
        string result;
        var diff = BigInteger.ValueOf((long) (difficulty * 255d));
        var quotient = RavencoinConstants.Diff1B.Divide(diff).Multiply(BigInteger.ValueOf(255));
        var bytes = quotient.ToByteArray().AsSpan();
        Span<byte> padded = stackalloc byte[RavencoinConstants.TargetPaddingLength];

        var padLength = RavencoinConstants.TargetPaddingLength - bytes.Length;

        if(padLength > 0)
        {
            bytes.CopyTo(padded.Slice(padLength, bytes.Length));
            result = padded.ToHexString(0, RavencoinConstants.TargetPaddingLength);
        }

        else
            result = bytes.ToHexString(0, RavencoinConstants.TargetPaddingLength);

        return result;
    }
     public static IDestination AddressToDestination(string address, Network expectedNetwork)
    {
        var decoded = Encoders.Base58Check.DecodeData(address);
        var networkVersionBytes = expectedNetwork.GetVersionBytes(Base58Type.PUBKEY_ADDRESS, true);
        decoded = decoded.Skip(networkVersionBytes.Length).ToArray();
        var result = new KeyId(decoded);

        return result;
    }

    public static IDestination BechSegwitAddressToDestination(string address, Network expectedNetwork)
    {
        var encoder = expectedNetwork.GetBech32Encoder(Bech32Type.WITNESS_PUBKEY_ADDRESS, true);
        var decoded = encoder.Decode(address, out var witVersion);
        var result = new WitKeyId(decoded);

        Debug.Assert(result.GetAddress(expectedNetwork).ToString() == address);
        return result;
    }

    public static IDestination BCashAddressToDestination(string address, Network expectedNetwork)
    {
        var bcash = NBitcoin.Altcoins.BCash.Instance.GetNetwork(expectedNetwork.ChainName);
        var trashAddress = bcash.Parse<NBitcoin.Altcoins.BCash.BTrashPubKeyAddress>(address);
        return trashAddress.ScriptPubKey.GetDestinationAddress(bcash);
    }
}
