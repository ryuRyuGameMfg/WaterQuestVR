/// <summary>
/// 水を受け取ることができるオブジェクトのインターフェース
/// WaterVesselから水を注がれた時に通知を受ける
/// </summary>
public interface IWaterReceiver
{
    /// <summary>
    /// 水を受け取る
    /// </summary>
    /// <param name="amount">水量</param>
    /// <param name="quality">水質</param>
    /// <returns>タスクが実行されたかどうか</returns>
    bool ReceiveWater(float amount, float quality);

    /// <summary>
    /// この受け手が水を受け取れる状態かどうか
    /// </summary>
    bool CanReceiveWater { get; }
}

