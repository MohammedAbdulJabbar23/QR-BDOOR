namespace AlBadour.Application.Common.Interfaces;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string content);
}
