using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Politics
{
    /// <summary>
    /// Domestic politics simulation with bill drafting, legislative processes,
    /// lobbying influence, polling dynamics, and media impact modeling.
    /// </summary>
    [Serializable]
    public struct Government : IComponentData
    {
        public int GovernmentID;
        public int NationID;
        public GovernmentForm Form;
        
        // Executive branch
        public int LeaderID;
        public float LeaderApproval; // 0-1
        public float LeaderPower; // Actual vs formal power
        public int TermStartTick;
        public int TermLengthTicks;
        public bool IsLame Duck;
        
        // Legislative branch
        public int LegislatureSeats;
        public NativeArray<int> PartySeatCounts;
        public float LegislativeEfficiency; // Bills passed / proposed
        public float GridlockIndex; // 0-1, higher = more gridlock
        
        // Judicial branch
        public float JudicialIndependence; // 0-1
        public float CourtPackingRisk; // 0-1
        
        // Overall governance
        public float GovernanceQuality; // 0-1 composite
        public float BureaucraticEfficiency; // 0-1
        public float PolicyImplementationRate; // 0-1
        public float RegulatoryQuality; // 0-1
    }
    
    public enum GovernmentForm
    {
        Presidential,
        Parliamentary,
        SemiPresidential,
        Directorial,
        Federal,
        Unitary,
        Confederal
    }
    
    [Serializable]
    public struct PoliticalParty : IComponentData
    {
        public int PartyID;
        public string PartyName;
        public IdeologyType Ideology;
        public int NationID;
        
        // Electoral metrics
        public float PopularSupport; // Current polling 0-1
        public float MembershipCount;
        public float Fundraising;
        public int SeatsHeld;
        public float SeatShare;
        
        // Party characteristics
        public float Cohesion; // Internal unity 0-1
        public float Discipline; // Voting discipline 0-1
        public float CharismaLeader; // Leader appeal 0-1
        public float OrganizationalStrength; // Ground game 0-1
        
        // Positioning
        public float EconomicLeftRight; // -1 (left) to +1 (right)
        public float SocialLiberalAuthoritarian; // -1 (liberal) to +1 (auth)
        public float PopulismLevel; // 0-1
        public float NationalismLevel; // 0-1
        
        // Dynamics
        public float Momentum; // Trend in support
        public float ScandalLevel; // Current scandals 0-1
        public float DefectionRisk; // Members leaving
    }
    
    public enum IdeologyType
    {
        Conservative,
        Liberal,
        Progressive,
        Socialist,
        Communist,
        Libertarian,
        Green,
        Nationalist,
        Populist,
        Centrist,
        ChristianDemocrat,
        SocialDemocrat,
        Fascist,
        Anarchist
    }
    
    [Serializable]
    public struct Legislation : IComponentData
    {
        public int BillID;
        public string Title;
        public string Description;
        public BillType Type;
        public int SponsorPartyID;
        public int[] CoSponsors;
        
        // Content dimensions
        public float EconomicImpact; // -1 (negative) to +1 (positive)
        public float SocialImpact; // -1 to +1
        public float EnvironmentalImpact; // -1 to +1
        public float FiscalCost; // Budget impact
        public float RegulatoryBurden; // 0-1
        
        // Legislative process
        public LegislativeStage CurrentStage;
        public float SupportLevel; // Current vote count / needed
        public float OppositionLevel;
        public float AmendmentCount;
        public bool IsControversial;
        public float ControversyLevel; // 0-1
        public int IntroducedTick;
        public int LastActionTick;
        
        // Influences
        public float LobbyingSupport; // Pro-lobbying $
        public float LobbyingOpposition; // Anti-lobbying $
        public float PublicSupport; // Polling 0-1
        public float MediaSupport; // Media sentiment -1 to +1
        public float ExecutiveSupport; // President/PM position -1 to +1
        
        // Outcome
        public bool WasPassed;
        public bool WasVetoed;
        public bool VetoOverridden;
        public int EnactmentTick;
    }
    
    public enum BillType
    {
        Economic,
        Social,
        Defense,
        Environmental,
        Healthcare,
        Education,
        Infrastructure,
        Tax,
        Regulatory,
        Constitutional,
        Emergency
    }
    
    public enum LegislativeStage
    {
        Drafting,
        CommitteeReview,
        SubcommitteeHearing,
        CommitteeVote,
        FloorDebate,
        FloorVote,
        OtherChamber,
        ConferenceCommittee,
        FinalPassage,
        ExecutiveReview,
        Enacted,
        Failed,
        Vetoed,
        Overridden
    }
    
    [Serializable]
    public struct LobbyingGroup : IComponentData
    {
        public int GroupID;
        public string Name;
        public InterestType InterestType;
        public float Budget;
        public float Influence; // 0-1
        public int LegislatorContacts; // Number of connections
        public float RevolvingDoorScore; // Former officials employed
        
        // Activities
        public float SpendingCurrentCycle;
        public float ADSpending;
        public float GrassrootsMobilization; // 0-1
        public float ThinkTankFunding;
        
        // Effectiveness
        public float SuccessRate; // Bills influenced / total
        public float AccessLevel; // 0-1, access to decision makers
        public float Credibility; // 0-1, perceived expertise
        
        // Positions on issues
        public NativeArray<int> SupportedBillIDs;
        public NativeArray<int> OpposedBillIDs;
    }
    
    public enum InterestType
    {
        Business,
        Labor,
        Professional,
        Ideological,
        SingleIssue,
        PublicInterest,
        Foreign,
        TradeAssociation
    }
    
    [Serializable]
    public struct PublicOpinion : IComponentData
    {
        public int RegionID;
        
        // Issue positions
        public float EconomicPolicyPreference; // -1 (left) to +1 (right)
        public float SocialPolicyPreference; // -1 (liberal) to +1 (conservative)
        public float ForeignPolicyPreference; // -1 (isolationist) to +1 (interventionist)
        public float EnvironmentalConcern; // 0-1
        
        // Government approval
        public float GovernmentApproval; // 0-1
        public float LegislatureApproval; // 0-1
        public float LeaderApproval; // 0-1
        
        // Mood indicators
        public float NationalMood; // -1 (pessimistic) to +1 (optimistic)
        public float ChangeDesire; // 0-1, desire for change
        public float PolarizationIndex; // 0-1
        
        // Demographic breakdowns
        public float YouthPreference; // Young voters
        public float ElderPreference; // Older voters
        public float UrbanPreference;
        public float RuralPreference;
        public float WorkingClassPreference;
        public float MiddleClassPreference;
        public float UpperClassPreference;
        
        // Media consumption
        public float TraditionalMediaTrust; // 0-1
        public float SocialMediaInfluence; // 0-1
        public float MisinformationExposure; // 0-1
        
        // Temporal
        public int LastPollTick;
        public float TrendDirection; // Rate of change
    }
    
    [Serializable]
    public struct MediaOutlet : IComponentData
    {
        public int OutletID;
        public string Name;
        public MediaType Type;
        public float Reach; // Audience size
        public float Credibility; // 0-1
        public float BiasScore; // -1 (left) to +1 (right)
        public float Sensationalism; // 0-1
        
        // Influence metrics
        public float AgendaSettingPower; // 0-1
        public float FramingPower; // 0-1
        public float PrimingEffect; // 0-1
        
        // Ownership
        public int OwnerID;
        public OwnershipType Ownership;
        public float CorporateInfluence; // 0-1
        public float GovernmentInfluence; // 0-1
        
        // Content
        public float NewsRatio; // News vs entertainment
        public float OpinionRatio;
        public float FactCheckScore; // Accuracy 0-1
    }
    
    public enum MediaType
    {
        Television,
        Newspaper,
        Radio,
        Online,
        SocialMedia,
        Podcast,
        NewsAgency
    }
    
    public enum OwnershipType
    {
        Private,
        Public,
        State,
        NonProfit,
        Cooperative,
        Conglomerate
    }
    
    public class DomesticPoliticsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Process legislative pipeline
            // Calculate lobbying influence
            // Update polling based on events
            // Model media impact on opinion
            // Simulate election cycles
            // Track party dynamics
        }
    }
}
