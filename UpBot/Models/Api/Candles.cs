using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpBot.Models.Api;

/// <summary>
/// 캔들 종류
/// </summary>
public enum CandleType
{
    /// <summary>
    /// 초
    /// </summary>
    Second,
    /// <summary>
    /// 분
    /// </summary>
    Minute,
    /// <summary>
    /// 일
    /// </summary>
    Day,
    /// <summary>
    /// 주
    /// </summary>
    Week,
    /// <summary>
    /// 월
    /// </summary>
    Month,
    /// <summary>
    /// 연
    /// </summary>
    Year
}

/// <summary>
/// 분 캔들 단위
/// </summary>
public enum CandleMinuteUnitType
{
    None = 0,
    Minute1 = 1,
    Minute3 = 3,
    Minute5 = 5,
    Minute10 = 10,
    Minute15 = 15,
    Minute30 = 30,
    Minute60 = 60,
    Minute240 = 240,
}


public class CandleBase : ApiResponseBase
{
    /// <summary>
    /// 페어(거래쌍)의 코드
    /// </summary>
    public string market { get; set; } = string.Empty;

    /// <summary>
    /// 캔들 구간의 시작 시각 (UTC 기준)
    /// </summary>
    public string candle_date_time_utc { get; set; } = string.Empty;

    /// <summary>
    /// 캔들 구간의 시작 시각 (KST 기준)
    /// </summary>
    public string candle_date_time_kst { get; set; } = string.Empty;

    /// <summary>
    /// 시가
    /// </summary>
    public double opening_price { get; set; }

    /// <summary>
    /// 고가
    /// </summary>
    public double high_price { get; set; }

    /// <summary>
    /// 저가
    /// </summary>
    public double low_price { get; set; }

    /// <summary>
    /// 종가
    /// </summary>
    public double trade_price { get; set; }

    /// <summary>
    /// 마지막 틱 저장 시각 (ms)
    /// </summary>
    public long timestamp { get; set; }

    /// <summary>
    /// 누적 거래 금액
    /// </summary>
    public double candle_acc_trade_price { get; set; }

    /// <summary>
    /// 누적 거래된 디지털 자산 수량
    /// </summary>
    public double candle_acc_trade_volume { get; set; }
}

/// <summary>
/// 초봉
/// </summary>
public class CandleSecond : CandleBase { }

public class CandleMinute : CandleBase
{
    /// <summary>
    /// 캔들 집계 시간 단위(분)
    /// </summary>
    public CandleMinuteUnitType unit { get; set; }
}

public class CandleDay : CandleBase
{
    /// <summary>
    /// 전일 종가 (UTC 0시 기준)
    /// </summary>
    public double prev_closing_price { get; set; }

    /// <summary>
    /// 전일 종가 대비 가격 변화.<br/>
    /// "trade_price" - "prev_closing_price"로 계산되며, 현재 종가가 전일 종가보다 얼마나 상승 또는 하락했는지를 나타냅니다.<br/>
    /// 양수(+): 현재 종가가 전일 종가보다 상승한 경우<br/>
    /// 음수(-) : 현재 종가가 전일 종가보다 하락한 경우
    /// </summary>
    public double change_price { get; set; }

    /// <summary>
    /// 전일 종가 대비 가격 변화율.<br/>
    /// ("trade_price" - "prev_closing_price") ÷ "prev_closing_price" 으로 계산됩니다.<br/>
    /// 양수(+): 가격 상승<br/>
    /// 음수(-) : 가격 하락
    /// </summary>
    public double change_rate { get; set; }
}

public class CandleWeek : CandleBase 
{
    /// <summary>
    /// 캔들 집계 시작일자 (yyyy-MM-dd)
    /// </summary>
    public string first_day_of_period { get; set; } = string.Empty;
}

public class CandleMonth : CandleBase 
{
    /// <summary>
    /// 캔들 집계 시작일자 (yyyy-MM-dd)
    /// </summary>
    public string first_day_of_period { get; set; } = string.Empty;
}

public class CandleYear : CandleBase 
{
    /// <summary>
    /// 캔들 집계 시작일자 (yyyy-MM-dd)
    /// </summary>
    public string first_day_of_period { get; set; } = string.Empty;
}
