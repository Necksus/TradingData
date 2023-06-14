using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace IPTMGrabber.YahooFinance
{
    // Alternative API?
    // https://query1.finance.yahoo.com/ws/fundamentals-timeseries/v1/finance/timeseries/CZR?lang=en-US&region=US&symbol=CZR&padTimeSeries=true&type=annualTaxEffectOfUnusualItems%2CtrailingTaxEffectOfUnusualItems%2CannualTaxRateForCalcs%2CtrailingTaxRateForCalcs%2CannualNormalizedEBITDA%2CtrailingNormalizedEBITDA%2CannualNormalizedDilutedEPS%2CtrailingNormalizedDilutedEPS%2CannualNormalizedBasicEPS%2CtrailingNormalizedBasicEPS%2CannualTotalUnusualItems%2CtrailingTotalUnusualItems%2CannualTotalUnusualItemsExcludingGoodwill%2CtrailingTotalUnusualItemsExcludingGoodwill%2CannualNetIncomeFromContinuingOperationNetMinorityInterest%2CtrailingNetIncomeFromContinuingOperationNetMinorityInterest%2CannualReconciledDepreciation%2CtrailingReconciledDepreciation%2CannualReconciledCostOfRevenue%2CtrailingReconciledCostOfRevenue%2CannualEBITDA%2CtrailingEBITDA%2CannualEBIT%2CtrailingEBIT%2CannualNetInterestIncome%2CtrailingNetInterestIncome%2CannualInterestExpense%2CtrailingInterestExpense%2CannualInterestIncome%2CtrailingInterestIncome%2CannualContinuingAndDiscontinuedDilutedEPS%2CtrailingContinuingAndDiscontinuedDilutedEPS%2CannualContinuingAndDiscontinuedBasicEPS%2CtrailingContinuingAndDiscontinuedBasicEPS%2CannualNormalizedIncome%2CtrailingNormalizedIncome%2CannualNetIncomeFromContinuingAndDiscontinuedOperation%2CtrailingNetIncomeFromContinuingAndDiscontinuedOperation%2CannualTotalExpenses%2CtrailingTotalExpenses%2CannualRentExpenseSupplemental%2CtrailingRentExpenseSupplemental%2CannualReportedNormalizedDilutedEPS%2CtrailingReportedNormalizedDilutedEPS%2CannualReportedNormalizedBasicEPS%2CtrailingReportedNormalizedBasicEPS%2CannualTotalOperatingIncomeAsReported%2CtrailingTotalOperatingIncomeAsReported%2CannualDividendPerShare%2CtrailingDividendPerShare%2CannualDilutedAverageShares%2CtrailingDilutedAverageShares%2CannualBasicAverageShares%2CtrailingBasicAverageShares%2CannualDilutedEPS%2CtrailingDilutedEPS%2CannualDilutedEPSOtherGainsLosses%2CtrailingDilutedEPSOtherGainsLosses%2CannualTaxLossCarryforwardDilutedEPS%2CtrailingTaxLossCarryforwardDilutedEPS%2CannualDilutedAccountingChange%2CtrailingDilutedAccountingChange%2CannualDilutedExtraordinary%2CtrailingDilutedExtraordinary%2CannualDilutedDiscontinuousOperations%2CtrailingDilutedDiscontinuousOperations%2CannualDilutedContinuousOperations%2CtrailingDilutedContinuousOperations%2CannualBasicEPS%2CtrailingBasicEPS%2CannualBasicEPSOtherGainsLosses%2CtrailingBasicEPSOtherGainsLosses%2CannualTaxLossCarryforwardBasicEPS%2CtrailingTaxLossCarryforwardBasicEPS%2CannualBasicAccountingChange%2CtrailingBasicAccountingChange%2CannualBasicExtraordinary%2CtrailingBasicExtraordinary%2CannualBasicDiscontinuousOperations%2CtrailingBasicDiscontinuousOperations%2CannualBasicContinuousOperations%2CtrailingBasicContinuousOperations%2CannualDilutedNIAvailtoComStockholders%2CtrailingDilutedNIAvailtoComStockholders%2CannualAverageDilutionEarnings%2CtrailingAverageDilutionEarnings%2CannualNetIncomeCommonStockholders%2CtrailingNetIncomeCommonStockholders%2CannualOtherunderPreferredStockDividend%2CtrailingOtherunderPreferredStockDividend%2CannualPreferredStockDividends%2CtrailingPreferredStockDividends%2CannualNetIncome%2CtrailingNetIncome%2CannualMinorityInterests%2CtrailingMinorityInterests%2CannualNetIncomeIncludingNoncontrollingInterests%2CtrailingNetIncomeIncludingNoncontrollingInterests%2CannualNetIncomeFromTaxLossCarryforward%2CtrailingNetIncomeFromTaxLossCarryforward%2CannualNetIncomeExtraordinary%2CtrailingNetIncomeExtraordinary%2CannualNetIncomeDiscontinuousOperations%2CtrailingNetIncomeDiscontinuousOperations%2CannualNetIncomeContinuousOperations%2CtrailingNetIncomeContinuousOperations%2CannualEarningsFromEquityInterestNetOfTax%2CtrailingEarningsFromEquityInterestNetOfTax%2CannualTaxProvision%2CtrailingTaxProvision%2CannualPretaxIncome%2CtrailingPretaxIncome%2CannualOtherIncomeExpense%2CtrailingOtherIncomeExpense%2CannualOtherNonOperatingIncomeExpenses%2CtrailingOtherNonOperatingIncomeExpenses%2CannualSpecialIncomeCharges%2CtrailingSpecialIncomeCharges%2CannualGainOnSaleOfPPE%2CtrailingGainOnSaleOfPPE%2CannualGainOnSaleOfBusiness%2CtrailingGainOnSaleOfBusiness%2CannualOtherSpecialCharges%2CtrailingOtherSpecialCharges%2CannualWriteOff%2CtrailingWriteOff%2CannualImpairmentOfCapitalAssets%2CtrailingImpairmentOfCapitalAssets%2CannualRestructuringAndMergernAcquisition%2CtrailingRestructuringAndMergernAcquisition%2CannualSecuritiesAmortization%2CtrailingSecuritiesAmortization%2CannualEarningsFromEquityInterest%2CtrailingEarningsFromEquityInterest%2CannualGainOnSaleOfSecurity%2CtrailingGainOnSaleOfSecurity%2CannualNetNonOperatingInterestIncomeExpense%2CtrailingNetNonOperatingInterestIncomeExpense%2CannualTotalOtherFinanceCost%2CtrailingTotalOtherFinanceCost%2CannualInterestExpenseNonOperating%2CtrailingInterestExpenseNonOperating%2CannualInterestIncomeNonOperating%2CtrailingInterestIncomeNonOperating%2CannualOperatingIncome%2CtrailingOperatingIncome%2CannualOperatingExpense%2CtrailingOperatingExpense%2CannualOtherOperatingExpenses%2CtrailingOtherOperatingExpenses%2CannualOtherTaxes%2CtrailingOtherTaxes%2CannualProvisionForDoubtfulAccounts%2CtrailingProvisionForDoubtfulAccounts%2CannualDepreciationAmortizationDepletionIncomeStatement%2CtrailingDepreciationAmortizationDepletionIncomeStatement%2CannualDepletionIncomeStatement%2CtrailingDepletionIncomeStatement%2CannualDepreciationAndAmortizationInIncomeStatement%2CtrailingDepreciationAndAmortizationInIncomeStatement%2CannualAmortization%2CtrailingAmortization%2CannualAmortizationOfIntangiblesIncomeStatement%2CtrailingAmortizationOfIntangiblesIncomeStatement%2CannualDepreciationIncomeStatement%2CtrailingDepreciationIncomeStatement%2CannualResearchAndDevelopment%2CtrailingResearchAndDevelopment%2CannualSellingGeneralAndAdministration%2CtrailingSellingGeneralAndAdministration%2CannualSellingAndMarketingExpense%2CtrailingSellingAndMarketingExpense%2CannualGeneralAndAdministrativeExpense%2CtrailingGeneralAndAdministrativeExpense%2CannualOtherGandA%2CtrailingOtherGandA%2CannualInsuranceAndClaims%2CtrailingInsuranceAndClaims%2CannualRentAndLandingFees%2CtrailingRentAndLandingFees%2CannualSalariesAndWages%2CtrailingSalariesAndWages%2CannualGrossProfit%2CtrailingGrossProfit%2CannualCostOfRevenue%2CtrailingCostOfRevenue%2CannualTotalRevenue%2CtrailingTotalRevenue%2CannualExciseTaxes%2CtrailingExciseTaxes%2CannualOperatingRevenue%2CtrailingOperatingRevenue&merge=false&period1=493590046&period2=1686319578&corsDomain=finance.yahoo.com

    // https://query1.finance.yahoo.com/ws/fundamentals-timeseries/v1/finance/timeseries/CZR?lang=en-US&region=US&symbol=CZR&padTimeSeries=true&type=annualTreasurySharesNumber%2CtrailingTreasurySharesNumber%2CannualPreferredSharesNumber%2CtrailingPreferredSharesNumber%2CannualOrdinarySharesNumber%2CtrailingOrdinarySharesNumber%2CannualShareIssued%2CtrailingShareIssued%2CannualNetDebt%2CtrailingNetDebt%2CannualTotalDebt%2CtrailingTotalDebt%2CannualTangibleBookValue%2CtrailingTangibleBookValue%2CannualInvestedCapital%2CtrailingInvestedCapital%2CannualWorkingCapital%2CtrailingWorkingCapital%2CannualNetTangibleAssets%2CtrailingNetTangibleAssets%2CannualCapitalLeaseObligations%2CtrailingCapitalLeaseObligations%2CannualCommonStockEquity%2CtrailingCommonStockEquity%2CannualPreferredStockEquity%2CtrailingPreferredStockEquity%2CannualTotalCapitalization%2CtrailingTotalCapitalization%2CannualTotalEquityGrossMinorityInterest%2CtrailingTotalEquityGrossMinorityInterest%2CannualMinorityInterest%2CtrailingMinorityInterest%2CannualStockholdersEquity%2CtrailingStockholdersEquity%2CannualOtherEquityInterest%2CtrailingOtherEquityInterest%2CannualGainsLossesNotAffectingRetainedEarnings%2CtrailingGainsLossesNotAffectingRetainedEarnings%2CannualOtherEquityAdjustments%2CtrailingOtherEquityAdjustments%2CannualFixedAssetsRevaluationReserve%2CtrailingFixedAssetsRevaluationReserve%2CannualForeignCurrencyTranslationAdjustments%2CtrailingForeignCurrencyTranslationAdjustments%2CannualMinimumPensionLiabilities%2CtrailingMinimumPensionLiabilities%2CannualUnrealizedGainLoss%2CtrailingUnrealizedGainLoss%2CannualTreasuryStock%2CtrailingTreasuryStock%2CannualRetainedEarnings%2CtrailingRetainedEarnings%2CannualAdditionalPaidInCapital%2CtrailingAdditionalPaidInCapital%2CannualCapitalStock%2CtrailingCapitalStock%2CannualOtherCapitalStock%2CtrailingOtherCapitalStock%2CannualCommonStock%2CtrailingCommonStock%2CannualPreferredStock%2CtrailingPreferredStock%2CannualTotalPartnershipCapital%2CtrailingTotalPartnershipCapital%2CannualGeneralPartnershipCapital%2CtrailingGeneralPartnershipCapital%2CannualLimitedPartnershipCapital%2CtrailingLimitedPartnershipCapital%2CannualTotalLiabilitiesNetMinorityInterest%2CtrailingTotalLiabilitiesNetMinorityInterest%2CannualTotalNonCurrentLiabilitiesNetMinorityInterest%2CtrailingTotalNonCurrentLiabilitiesNetMinorityInterest%2CannualOtherNonCurrentLiabilities%2CtrailingOtherNonCurrentLiabilities%2CannualLiabilitiesHeldforSaleNonCurrent%2CtrailingLiabilitiesHeldforSaleNonCurrent%2CannualRestrictedCommonStock%2CtrailingRestrictedCommonStock%2CannualPreferredSecuritiesOutsideStockEquity%2CtrailingPreferredSecuritiesOutsideStockEquity%2CannualDerivativeProductLiabilities%2CtrailingDerivativeProductLiabilities%2CannualEmployeeBenefits%2CtrailingEmployeeBenefits%2CannualNonCurrentPensionAndOtherPostretirementBenefitPlans%2CtrailingNonCurrentPensionAndOtherPostretirementBenefitPlans%2CannualNonCurrentAccruedExpenses%2CtrailingNonCurrentAccruedExpenses%2CannualDuetoRelatedPartiesNonCurrent%2CtrailingDuetoRelatedPartiesNonCurrent%2CannualTradeandOtherPayablesNonCurrent%2CtrailingTradeandOtherPayablesNonCurrent%2CannualNonCurrentDeferredLiabilities%2CtrailingNonCurrentDeferredLiabilities%2CannualNonCurrentDeferredRevenue%2CtrailingNonCurrentDeferredRevenue%2CannualNonCurrentDeferredTaxesLiabilities%2CtrailingNonCurrentDeferredTaxesLiabilities%2CannualLongTermDebtAndCapitalLeaseObligation%2CtrailingLongTermDebtAndCapitalLeaseObligation%2CannualLongTermCapitalLeaseObligation%2CtrailingLongTermCapitalLeaseObligation%2CannualLongTermDebt%2CtrailingLongTermDebt%2CannualLongTermProvisions%2CtrailingLongTermProvisions%2CannualCurrentLiabilities%2CtrailingCurrentLiabilities%2CannualOtherCurrentLiabilities%2CtrailingOtherCurrentLiabilities%2CannualCurrentDeferredLiabilities%2CtrailingCurrentDeferredLiabilities%2CannualCurrentDeferredRevenue%2CtrailingCurrentDeferredRevenue%2CannualCurrentDeferredTaxesLiabilities%2CtrailingCurrentDeferredTaxesLiabilities%2CannualCurrentDebtAndCapitalLeaseObligation%2CtrailingCurrentDebtAndCapitalLeaseObligation%2CannualCurrentCapitalLeaseObligation%2CtrailingCurrentCapitalLeaseObligation%2CannualCurrentDebt%2CtrailingCurrentDebt%2CannualOtherCurrentBorrowings%2CtrailingOtherCurrentBorrowings%2CannualLineOfCredit%2CtrailingLineOfCredit%2CannualCommercialPaper%2CtrailingCommercialPaper%2CannualCurrentNotesPayable%2CtrailingCurrentNotesPayable%2CannualPensionandOtherPostRetirementBenefitPlansCurrent%2CtrailingPensionandOtherPostRetirementBenefitPlansCurrent%2CannualCurrentProvisions%2CtrailingCurrentProvisions%2CannualPayablesAndAccruedExpenses%2CtrailingPayablesAndAccruedExpenses%2CannualCurrentAccruedExpenses%2CtrailingCurrentAccruedExpenses%2CannualInterestPayable%2CtrailingInterestPayable%2CannualPayables%2CtrailingPayables%2CannualOtherPayable%2CtrailingOtherPayable%2CannualDuetoRelatedPartiesCurrent%2CtrailingDuetoRelatedPartiesCurrent%2CannualDividendsPayable%2CtrailingDividendsPayable%2CannualTotalTaxPayable%2CtrailingTotalTaxPayable%2CannualIncomeTaxPayable%2CtrailingIncomeTaxPayable%2CannualAccountsPayable%2CtrailingAccountsPayable%2CannualTotalAssets%2CtrailingTotalAssets%2CannualTotalNonCurrentAssets%2CtrailingTotalNonCurrentAssets%2CannualOtherNonCurrentAssets%2CtrailingOtherNonCurrentAssets%2CannualDefinedPensionBenefit%2CtrailingDefinedPensionBenefit%2CannualNonCurrentPrepaidAssets%2CtrailingNonCurrentPrepaidAssets%2CannualNonCurrentDeferredAssets%2CtrailingNonCurrentDeferredAssets%2CannualNonCurrentDeferredTaxesAssets%2CtrailingNonCurrentDeferredTaxesAssets%2CannualDuefromRelatedPartiesNonCurrent%2CtrailingDuefromRelatedPartiesNonCurrent%2CannualNonCurrentNoteReceivables%2CtrailingNonCurrentNoteReceivables%2CannualNonCurrentAccountsReceivable%2CtrailingNonCurrentAccountsReceivable%2CannualFinancialAssets%2CtrailingFinancialAssets%2CannualInvestmentsAndAdvances%2CtrailingInvestmentsAndAdvances%2CannualOtherInvestments%2CtrailingOtherInvestments%2CannualInvestmentinFinancialAssets%2CtrailingInvestmentinFinancialAssets%2CannualHeldToMaturitySecurities%2CtrailingHeldToMaturitySecurities%2CannualAvailableForSaleSecurities%2CtrailingAvailableForSaleSecurities%2CannualFinancialAssetsDesignatedasFairValueThroughProfitorLossTotal%2CtrailingFinancialAssetsDesignatedasFairValueThroughProfitorLossTotal%2CannualTradingSecurities%2CtrailingTradingSecurities%2CannualLongTermEquityInvestment%2CtrailingLongTermEquityInvestment%2CannualInvestmentsinJointVenturesatCost%2CtrailingInvestmentsinJointVenturesatCost%2CannualInvestmentsInOtherVenturesUnderEquityMethod%2CtrailingInvestmentsInOtherVenturesUnderEquityMethod%2CannualInvestmentsinAssociatesatCost%2CtrailingInvestmentsinAssociatesatCost%2CannualInvestmentsinSubsidiariesatCost%2CtrailingInvestmentsinSubsidiariesatCost%2CannualInvestmentProperties%2CtrailingInvestmentProperties%2CannualGoodwillAndOtherIntangibleAssets%2CtrailingGoodwillAndOtherIntangibleAssets%2CannualOtherIntangibleAssets%2CtrailingOtherIntangibleAssets%2CannualGoodwill%2CtrailingGoodwill%2CannualNetPPE%2CtrailingNetPPE%2CannualAccumulatedDepreciation%2CtrailingAccumulatedDepreciation%2CannualGrossPPE%2CtrailingGrossPPE%2CannualLeases%2CtrailingLeases%2CannualConstructionInProgress%2CtrailingConstructionInProgress%2CannualOtherProperties%2CtrailingOtherProperties%2CannualMachineryFurnitureEquipment%2CtrailingMachineryFurnitureEquipment%2CannualBuildingsAndImprovements%2CtrailingBuildingsAndImprovements%2CannualLandAndImprovements%2CtrailingLandAndImprovements%2CannualProperties%2CtrailingProperties%2CannualCurrentAssets%2CtrailingCurrentAssets%2CannualOtherCurrentAssets%2CtrailingOtherCurrentAssets%2CannualHedgingAssetsCurrent%2CtrailingHedgingAssetsCurrent%2CannualAssetsHeldForSaleCurrent%2CtrailingAssetsHeldForSaleCurrent%2CannualCurrentDeferredAssets%2CtrailingCurrentDeferredAssets%2CannualCurrentDeferredTaxesAssets%2CtrailingCurrentDeferredTaxesAssets%2CannualRestrictedCash%2CtrailingRestrictedCash%2CannualPrepaidAssets%2CtrailingPrepaidAssets%2CannualInventory%2CtrailingInventory%2CannualInventoriesAdjustmentsAllowances%2CtrailingInventoriesAdjustmentsAllowances%2CannualOtherInventories%2CtrailingOtherInventories%2CannualFinishedGoods%2CtrailingFinishedGoods%2CannualWorkInProcess%2CtrailingWorkInProcess%2CannualRawMaterials%2CtrailingRawMaterials%2CannualReceivables%2CtrailingReceivables%2CannualReceivablesAdjustmentsAllowances%2CtrailingReceivablesAdjustmentsAllowances%2CannualOtherReceivables%2CtrailingOtherReceivables%2CannualDuefromRelatedPartiesCurrent%2CtrailingDuefromRelatedPartiesCurrent%2CannualTaxesReceivable%2CtrailingTaxesReceivable%2CannualAccruedInterestReceivable%2CtrailingAccruedInterestReceivable%2CannualNotesReceivable%2CtrailingNotesReceivable%2CannualLoansReceivable%2CtrailingLoansReceivable%2CannualAccountsReceivable%2CtrailingAccountsReceivable%2CannualAllowanceForDoubtfulAccountsReceivable%2CtrailingAllowanceForDoubtfulAccountsReceivable%2CannualGrossAccountsReceivable%2CtrailingGrossAccountsReceivable%2CannualCashCashEquivalentsAndShortTermInvestments%2CtrailingCashCashEquivalentsAndShortTermInvestments%2CannualOtherShortTermInvestments%2CtrailingOtherShortTermInvestments%2CannualCashAndCashEquivalents%2CtrailingCashAndCashEquivalents%2CannualCashEquivalents%2CtrailingCashEquivalents%2CannualCashFinancial%2CtrailingCashFinancial&merge=false&period1=493590046&period2=1686320072&corsDomain=finance.yahoo.com

    //https://query1.finance.yahoo.com/ws/fundamentals-timeseries/v1/finance/timeseries/CZR?lang=en-US&region=US&symbol=CZR&padTimeSeries=true&type=annualEBIT&merge=false&period1=1672444800&period2=1672444800&corsDomain=finance.yahoo.com

    // Taken from : https://cryptocointracker.com/yahoo-finance/yahoo-finance-api
    public enum YahooModule
    {
        AssetProfile,
        DefaultKeyStatistics,
        RecommendationTrend,
        FinancialData,
        MajorHoldersBreakdown,
        Earnings,
        EarningsHistory,
        EarningsTrend,
        IndexTrend,
        IndustryTrend,
        NetSharePurchaseActivity,
        SectorTrend,
        InsiderHolders,
        UpgradeDowngradeHistory,
    }

    internal class YahooGrabber
    {

        public async Task ExecuteAsync(string dataRoot)
        {
            // Prepare reader
            using var reader = new StreamReader(Path.Combine(dataRoot, "Zacks", "Screener.csv"));
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ","
            }) ;
            await csv.ReadAsync();
            csv.ReadHeader();

            // Prepare writer
            using var client = new HttpClient();
            await using var writer = new StreamWriter(Path.Combine(dataRoot, "YahooFinance", "ScreenerDetails.csv"));
            await using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
            });
            csvWriter.WriteHeader<QuoteDetail>();
            await csvWriter.NextRecordAsync();

            // Dump data from Yahoo Finance
            while (await csv.ReadAsync())
            {
                var ticker = FixTicker(csv.GetField<string>("Ticker")!);

                HttpResponseMessage response = null;
                bool error;

                do
                {
                    error = false;
                    try
                    {
                        response = await client.GetAsync(GetUrl(ticker, YahooModule.AssetProfile));
                    }
                    catch (Exception ex)
                    {
                        error = true;
                    }
                } while (error);

                if (response.IsSuccessStatusCode)
                {
                    var quoteDetail = QuoteDetail.FromJson(ticker, await response.Content.ReadAsStringAsync());
                    csvWriter.WriteRecord(quoteDetail);
                    await csvWriter.NextRecordAsync();
                    Console.WriteLine($"{ticker} : {quoteDetail.WebSite}");
                }
                else
                {
                    Console.WriteLine($"Erreur HTTP : {response.StatusCode}, ticker {ticker}");
                }

                //TimeSerieType.annualTotalAssets + TimeSerieType.annualInterestExpense + TimeSerieType.annualWorkingCapital
            }

        }


        private string FixTicker(string ticker)
        {
            var cleanTicker = ticker.Replace(".", "-");
            switch (cleanTicker)
            {
                case "CWENA":
                    return "CWEN";
                case "GRP-U":
                    return "GRP-UN";
                default:
                    return cleanTicker;
            }
        }

        string GetUrl(string ticker, params YahooModule[] modules)
            => $"https://query1.finance.yahoo.com/v10/finance/quoteSummary/{ticker}?modules={string.Join(",", modules)}";
    }
}
