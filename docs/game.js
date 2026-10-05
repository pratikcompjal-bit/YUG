/**
 * YUG — Chola Dynasty Game Engine
 * Rajendra's Realm: 11th Century Historical Kingdom Chronicle
 */

(function () {
  const INITIAL_STATE = {
    yearIndex: 0,
    regnalYear: 1,
    calYear: 1014,
    sovereign: 'Rajendra Chola I',
    title: 'Madhurantakan / Parakesarivarman',
    resources: {
      grain: 1200,   // Kalam of paddy
      gold: 850,     // Kasus
      granite: 320,  // Dressed stone blocks
      fleet: 4       // Karavai merchant & war ships
    },
    metrics: {
      grandeur: 20,   // Dharma & Temple majesty (0-100)
      hydraulic: 25,  // Kaveri waterworks & agriculture (0-100)
      maritime: 15,   // Naval prestige & overseas trade (0-100)
      trust: 60       // Assembly & village harmony (0-100)
    },
    provinces: {
      thanjavur: {
        id: 'thanjavur',
        name: 'Thanjavur',
        tagline: 'Temple City & Epigraphic Capital',
        tier: 1,
        tierName: 'Upapitha & Great Plinth Completed',
        vimanaHeight: 70, // feet
        bronzesCast: 1,
        projects: [
          {
            id: 'thanjavur_vimana_1',
            title: 'Erect Granite Tower Levels 1–5',
            cost: { granite: 140, gold: 200, grain: 150 },
            gain: { grandeur: 18, trust: 6 },
            completed: false,
            desc: 'Quarry hard granite from Namakkal and haul dressed blocks over earthen ramps.'
          },
          {
            id: 'thanjavur_vimana_2',
            title: 'Raise the 80-Tonne Capstone & Golden Kalasam',
            cost: { granite: 220, gold: 350, grain: 220 },
            gain: { grandeur: 28, trust: 10 },
            completed: false,
            requires: 'thanjavur_vimana_1',
            desc: 'Crown the Brihadisvara tower at 216 feet with a single-block granite sikhara.'
          },
          {
            id: 'thanjavur_bronze',
            title: 'Cast Sacred Chola Bronzes (Lost-Wax)',
            cost: { gold: 160, grain: 80 },
            gain: { grandeur: 14, trust: 5 },
            completed: false,
            desc: 'Commission sthapatis to cast masterworks of Nataraja and Tripurantaka.'
          }
        ]
      },
      gangaikonda: {
        id: 'gangaikonda',
        name: 'Gangaikonda Cholapuram',
        tagline: 'Imperial Seat & Great Waterworks',
        tier: 1,
        tierName: 'Surveying the New Capital',
        canalMiles: 12,
        reservoirCapacity: 35, // %
        projects: [
          {
            id: 'gangaikonda_lake_1',
            title: 'Excavate the Cholaganga Reservoir (16 Miles)',
            cost: { granite: 160, gold: 220, grain: 260 },
            gain: { hydraulic: 25, trust: 8 },
            completed: false,
            desc: 'Construct earthen embankments and stone sluices to store Kaveri floodwaters.'
          },
          {
            id: 'gangaikonda_canals',
            title: 'Kallanai Tributary Canals & Sluices',
            cost: { granite: 110, gold: 150, grain: 180 },
            gain: { hydraulic: 20, grainYield: 150 },
            completed: false,
            desc: 'Carve distribution channels feeding hundreds of delta paddy villages.'
          },
          {
            id: 'gangaikonda_palace',
            title: 'Raise Mudikondan Royal Palace & Granary',
            cost: { granite: 180, gold: 300, grain: 150 },
            gain: { grandeur: 18, trust: 12 },
            completed: false,
            desc: 'Stone and timber administrative courts for royal scribes, treasurers, and army marshals.'
          }
        ]
      },
      nagapattinam: {
        id: 'nagapattinam',
        name: 'Nagapattinam',
        tagline: 'Ocean Port & Maritime Gateway',
        tier: 1,
        tierName: 'Harbor of the Merchant Guilds',
        tradeVessels: 4,
        foreignEnvoys: 1,
        projects: [
          {
            id: 'nagapattinam_port_1',
            title: 'Deep-Water Jetty & Coral Breakwater',
            cost: { granite: 120, gold: 200, grain: 100 },
            gain: { maritime: 20, fleet: 1 },
            completed: false,
            desc: 'Construct stone docks accommodating deep-keeled vessels arriving from Sumatra.'
          },
          {
            id: 'nagapattinam_vihara',
            title: 'Charter the Chudamani Buddhist Vihara',
            cost: { gold: 180, grain: 120 },
            gain: { maritime: 15, trust: 10 },
            completed: false,
            desc: 'Grant tax-free villages (*Brahmadeya/Devadana*) to patronize Srivijayan and international monks.'
          },
          {
            id: 'nagapattinam_shipyard',
            title: 'Royal Karavai Naval Shipyards',
            cost: { granite: 150, gold: 320, grain: 200 },
            gain: { maritime: 25, fleet: 2 },
            completed: false,
            desc: 'Fell teak from Anaimalai hills to build multi-oared warships and trade galleons.'
          }
        ]
      }
    },
    decreesHistory: [],
    expeditionsHistory: [],
    ballotsHistory: []
  };

  const HISTORICAL_DECREES = [
    {
      year: 1,
      calYear: 1014,
      title: 'Ascension & Revenue Survey of the Delta',
      tamil: 'நில அளவை மற்றும் வரி நெறிமுறை',
      kicker: 'REGNAL YEAR 01 · 1014 CE',
      context: 'As you assume sole sovereignty, the imperial land surveyors (*Kankani*) complete the palm-leaf registers of agricultural yields across the Kaveri basin. How will you fix the royal grain tithe (*Kadamai*)?',
      choices: [
        {
          label: 'Fix Moderate Tithe & Fund Village Maintenance',
          summary: 'Demand 1/6th grain yield; leave surplus for village tank cleaning and school teachers.',
          cost: {},
          effect: { grain: 240, gold: 120, trust: 12, hydraulic: 5 },
          log: 'The village assemblies praised the imperial leniency; community irrigation ditches were cleared before monsoons.'
        },
        {
          label: 'Maximize Granary Reserves for Royal Infrastructure',
          summary: 'Levy full 1/3rd tithe to accelerate stone quarries and temple construction.',
          cost: {},
          effect: { grain: 450, gold: 260, granite: 60, trust: -10 },
          log: 'Royal granaries overflowed with paddy, though village elders grumbled at the heavy measure.'
        },
        {
          label: 'Grant Tax Exemptions to Temple Deities & Scholars',
          summary: 'Designate rich wetlands as tax-free *Devadana* lands dedicated to Brihadisvara.',
          cost: {},
          effect: { gold: 80, grain: 100, grandeur: 16, trust: 6 },
          log: 'The priests chanted Vedic and Tevaram hymns praising the sovereign; arts flourished across the delta.'
        }
      ]
    },
    {
      year: 2,
      calYear: 1015,
      title: 'The Kudavolai Assembly of Uttiramerur',
      tamil: 'உத்திரமேரூர் குடவோலை முறை',
      kicker: 'REGNAL YEAR 02 · 1015 CE',
      context: 'The village assembly gathers under the temple mandapa to choose the Annual Committee (*Samvatsara-variyam*) and Tank Committee (*Eri-variyam*). Candidates must own tax-paying land, know the hymns, and be incorruptible.',
      interactiveType: 'kudavolai',
      choices: [
        {
          label: 'Ratify Kudavolai Democratic Ballot Selection',
          summary: 'Supervise the drawing of written palm leaves from a brass pot by a child.',
          cost: { grain: 50 },
          effect: { trust: 20, hydraulic: 12 },
          log: 'The pot-lot ballots selected righteous elders; community water management achieved historic efficiency.'
        },
        {
          label: 'Appoint Imperial Commissioners Directly',
          summary: 'Place crown officers (*Adhikari*) in charge of delta sluice gates.',
          cost: { gold: 80 },
          effect: { hydraulic: 16, gold: 90, trust: -12 },
          log: 'Hydraulic control was centralized, but village elders resented imperial oversight.'
        }
      ]
    },
    {
      year: 3,
      calYear: 1016,
      title: 'The Great Kaveri Monsoon Flood',
      tamil: 'காவிரி வெள்ளப் பெருக்கு',
      kicker: 'REGNAL YEAR 03 · 1016 CE',
      context: 'Raging monsoon torrents swell the northern branch of the Kaveri (Coleroon). Floodwaters threaten to submerge thousand-acre ripening paddy fields near Thanjavur.',
      choices: [
        {
          label: 'Deploy Royal Army to Reinforce Embankments with Granite',
          summary: 'Send elephant brigades and quarry stone to raise massive stone dykes along the riverbanks.',
          cost: { granite: 80, grain: 120 },
          effect: { hydraulic: 22, trust: 15, grain: 200 },
          log: 'The granite embankments held firm against the surging flood. The crop was saved and thousands cheered.'
        },
        {
          label: 'Breach Secondary Canals into Flood Spillways',
          summary: 'Sacrifice downstream marshes to preserve the royal city center.',
          cost: { gold: 60 },
          effect: { grain: -100, hydraulic: 8, trust: -5 },
          log: 'The capital was spared, but peripheral marsh villages lost their winter harvest.'
        },
        {
          label: 'Open Imperial Granaries for Immediate Famine Relief',
          summary: 'Distribute cooked rice (*Kanchi*) and seed grains freely to all displaced peasants.',
          cost: { grain: 220, gold: 50 },
          effect: { trust: 25, grandeur: 10 },
          log: 'Songs of imperial compassion spread to the western hills; not a single soul perished of hunger.'
        }
      ]
    },
    {
      year: 4,
      calYear: 1017,
      title: 'The Anuradhapura & Southern Seas Mission',
      tamil: 'ஈழ மண்டல அமைதித் திட்டம்',
      kicker: 'REGNAL YEAR 04 · 1017 CE',
      context: 'Southern territories in Sri Lanka (*Ila-mandalam*) are stabilized. Royal generals report captured regalia and gem mines around Polonnaruwa. How will these spoils be dedicated?',
      choices: [
        {
          label: 'Endow Brihadisvara with Ceylon Rubies & Pearl Garlands',
          summary: 'Adorn the presiding deity with consecrated gemstones and endow eternal oil lamps.',
          cost: {},
          effect: { grandeur: 24, gold: 180, trust: 8 },
          log: 'The great shrine glowed with hundreds of perpetual ghee lamps, captivating visitors from across Asia.'
        },
        {
          label: 'Reinvest in Coromandel Port Fortifications & Naval Yards',
          summary: 'Direct wealth toward expanding shipwright facilities at Nagapattinam.',
          cost: { grain: 100 },
          effect: { maritime: 22, fleet: 1, gold: 120 },
          log: 'Shipbuilders laid keels for broad-beamed war galleons capable of ocean navigation.'
        },
        {
          label: 'Distribute Seed Loans & Rebuild Southern Buddhist Temples',
          summary: 'Respect local traditions and reconstruct holy dagobas destroyed in past campaigns.',
          cost: { gold: 90 },
          effect: { trust: 18, maritime: 10, grandeur: 8 },
          log: 'Peace was cemented across the straits; merchant caravans moved unimpeded between island and mainland.'
        }
      ]
    },
    {
      year: 5,
      calYear: 1018,
      title: 'The Ayyavole 500 Merchant Guild Petition',
      tamil: 'திசையாயிரத்து ஐந்நூற்றுவர் வணிக சாசனம்',
      kicker: 'REGNAL YEAR 05 · 1018 CE',
      context: 'The renowned transnational guild *Ainurruvar* (The 500 of the Thousand Directions) requests royal seals and tax remissions to establish fortified trade depots (*Erivirapattinam*) at Nagapattinam.',
      choices: [
        {
          label: 'Grant Royal Copper Plate Charter & Guild Privileges',
          summary: 'Permit the guild to fly the imperial tiger banner in exchange for overseas trade customs.',
          cost: { gold: 50 },
          effect: { maritime: 24, gold: 320, trust: 8 },
          log: 'Cardamom, camphor, ivory, and Chinese porcelain flooded Nagapattinam markets; customs revenues doubled.'
        },
        {
          label: 'Enforce Heavy Tariffs on Foreign Luxury Cargoes',
          summary: 'Demand high port dues on imported silks, resins, and Arabian thoroughbred horses.',
          cost: {},
          effect: { gold: 240, maritime: -8, trust: -4 },
          log: 'Short-term treasury gains were substantial, though some merchant caravans diverted to Malabar ports.'
        },
        {
          label: 'Partner with Guild to Fund State-Owned Spice Caravels',
          summary: 'Jointly invest royal gold with guild merchants for shared maritime ventures.',
          cost: { gold: 150, grain: 100 },
          effect: { maritime: 28, fleet: 1, gold: 280 },
          log: 'The Chola merchant armada sailed with royal escorts, establishing commanding presence in the Bay.'
        }
      ]
    },
    {
      year: 6,
      calYear: 1019,
      title: 'The Srivijayan Trade Voyage to the Eastern Archipelago',
      tamil: 'ஸ்ரீவிஜய கடல் வணிகப் பயணம்',
      kicker: 'REGNAL YEAR 06 · 1019 CE',
      context: 'The monsoon winds blow steadily eastward toward the Straits of Malacca. A royal commercial armada is prepared at Nagapattinam to sail to Sumatra and Kedah (*Kadaram*).',
      interactiveType: 'expedition',
      choices: [
        {
          label: 'Dispatch High-Value Commercial Fleet with Naval Guard',
          summary: 'Load fine cottons, bronze deities, and salt; escort with fast Karavai warships.',
          cost: { gold: 120, grain: 150 },
          effect: { maritime: 26, gold: 380, grandeur: 10 },
          log: 'The fleet anchored at the Straits of Malacca; the Maharaja of Srivijaya sent gold robes in greeting.'
        },
        {
          label: 'Retain Fleet for Coastal Defense & Patrol',
          summary: 'Guard Coromandel ports against regional pirates and storm damage.',
          cost: {},
          effect: { trust: 10, maritime: 6, grain: 80 },
          log: 'The Coromandel coast remained peaceful and secure, though lucrative overseas opportunities were delayed.'
        }
      ]
    },
    {
      year: 7,
      calYear: 1021,
      title: 'The Expedition to the Sacred River Ganga',
      tamil: 'கங்கை கொண்ட சோழன் பேரணி',
      kicker: 'REGNAL YEAR 07 · 1021 CE',
      context: 'Chola armies under General Araiyan Rajarajan return from the northern plains of Bengal. They bear golden urns filled with the sacred water of the River Ganga to sanctify your new imperial capital.',
      choices: [
        {
          label: 'Consecrate the Great Cholaganga Lake as a "Liquid Pillar of Victory"',
          summary: 'Pour the holy waters into the 16-mile reservoir and proclaim the title *Gangaikonda Cholan*.',
          cost: { granite: 120, gold: 200, grain: 200 },
          effect: { grandeur: 35, hydraulic: 30, trust: 20 },
          log: 'The vast Cholaganga lake shone like a sea; poets and pilgrims hailed the triumphant sovereign across India.'
        },
        {
          label: 'Commence Construction of the Sister Brihadisvara Temple',
          summary: 'Lay foundations for the majestic temple of Gangaikonda Cholapuram with curved feminine grace.',
          cost: { granite: 200, gold: 300, grain: 200 },
          effect: { grandeur: 40, trust: 15 },
          log: 'The rising temple rivaled Thanjavur in beauty, featuring exquisite relief carvings of Shiva and Saraswati.'
        },
        {
          label: 'Fund Grand Feast for 100,000 Laborers & Veterans',
          summary: 'Distribute cooked rice, ghee sweets, and land grants (*Kani*) to every soldier and stonecutter.',
          cost: { grain: 350, gold: 150 },
          effect: { trust: 35, grandeur: 15 },
          log: 'From coastal fishermen to forest hunters, the people sang ballads honoring the generosity of their King.'
        }
      ]
    },
    {
      year: 8,
      calYear: 1022,
      title: 'The Masterpiece: Ananda Tandava Nataraja',
      tamil: 'ஆனந்த தாண்டவ நடராஜர் வார்ப்பு',
      kicker: 'REGNAL YEAR 08 · 1022 CE',
      context: 'Chief royal sculptor (*Perunthachan*) presents the completed beeswax model of Nataraja dancing inside a ring of flames (*Tiruvasi*). Pouring the molten bronze alloy requires royal patronage.',
      choices: [
        {
          label: 'Cast in Sacred Five-Metal Alloy (*Panchaloha*) with Royal Gold',
          summary: 'Blend copper, zinc, lead, silver, and gold to cast an immortal 8-foot sculpture.',
          cost: { gold: 220, grain: 100 },
          effect: { grandeur: 32, trust: 14 },
          log: 'The bronze was pulled from the mould flawless and radiant—the cosmic dance captured forever in metal.'
        },
        {
          label: 'Standardize Temple Bronze Foundries Across All Districts',
          summary: 'Establish permanent casting guilds in Kumbakonam and Swamimalai.',
          cost: { gold: 160, grain: 120 },
          effect: { grandeur: 22, gold: 150, trust: 10 },
          log: 'Dozens of shrines received sacred bronzes, establishing a stylistic golden age that survived a millennium.'
        },
        {
          label: 'Conserve Metal for Naval Anchor Chains & Armor',
          summary: 'Direct copper and bronze to shipbuilders and infantry regiments.',
          cost: { gold: 80 },
          effect: { fleet: 1, maritime: 15, trust: -6 },
          log: 'Naval readiness was bolstered, though temple patrons lamented the delay of the sacred icon.'
        }
      ]
    },
    {
      year: 9,
      calYear: 1024,
      title: 'Embassies to Song China & Canton Ports',
      tamil: 'சீன சாங் பேரரசு உடனான தூதரகம்',
      kicker: 'REGNAL YEAR 09 · 1024 CE',
      context: 'An imperial embassy arrives back from the court of Song Emperor Zhenzong in Kaifeng. The Chinese court has granted the Chola monarch tribute status of highest dignity and opened the Canton customs.',
      choices: [
        {
          label: 'Establish Bilateral Silk, Glass & Spices Monopoly',
          summary: 'Station permanent Chola trade emissaries in Canton and Quanzhou.',
          cost: { gold: 180, grain: 100 },
          effect: { maritime: 30, gold: 460, trust: 10 },
          log: 'The trade route became the richest maritime artery on earth; Song coins and celadon ware filled Chola treasuries.'
        },
        {
          label: 'Charter Buddhist Pilgrimage Escorts for Chinese Monks',
          summary: 'Facilitate safe ocean passage and vihara stays for Chinese scholars visiting Bodh Gaya.',
          cost: { grain: 140, gold: 80 },
          effect: { grandeur: 20, maritime: 18, trust: 15 },
          log: 'Chinese annals recorded praise for Chola piety, justice, and oceanic maritime safety.'
        },
        {
          label: 'Levy Luxury Dues on Foreign Chinese Porcelains',
          summary: 'Collect high tariffs at Nagapattinam quay before goods move inland.',
          cost: {},
          effect: { gold: 300, maritime: 5 },
          log: 'The royal treasury bulged with precious metals and fine glazed porcelain vases.'
        }
      ]
    },
    {
      year: 10,
      calYear: 1025,
      title: 'The Great Kadaram Expedition',
      tamil: 'கடாரம் கொண்ட மாபெரும் கடற்படைப் போர்',
      kicker: 'REGNAL YEAR 10 · 1025 CE',
      context: 'The maritime confederacy of Srivijaya attempts to blockade the Malacca and Sunda straits, levying unlawful extortions on Indian merchant ships. Royal admirals await the command to launch the legendary naval assault.',
      choices: [
        {
          label: 'Deploy the Full Imperial Armada to Secure the Straits',
          summary: 'Dispatch hundreds of warships to liberate Kedah (*Kadaram*), Pannai, and Malaiyur.',
          cost: { fleet: 2, gold: 260, grain: 300 },
          effect: { maritime: 45, grandeur: 30, trust: 25 },
          log: 'The armada struck with invincible swiftness. Kadaram was captured, the straits opened, and Rajendra earned the immortal title KADARAM KONDAN.'
        },
        {
          label: 'Negotiate Armed Neutrality & Establish Naval Station at Nicobar',
          summary: 'Garrison the Andaman & Nicobar islands (*Nakkavaram*) to escort merchant ships safely.',
          cost: { gold: 150, grain: 180 },
          effect: { maritime: 25, trust: 12, gold: 200 },
          log: 'A fortified naval outpost secured trade convoys while avoiding prolonged overseas war.'
        },
        {
          label: 'Focus Naval Power on Inland Canal Defense & Coasts',
          summary: 'Keep the fleet anchored along the Coromandel coast for domestic protection.',
          cost: {},
          effect: { trust: 10, maritime: -15, grain: 150 },
          log: 'Domestic security remained unchallenged, but overseas trade influence waned as foreign fleets seized the straits.'
        }
      ]
    }
  ];

  class CholaGameEngine {
    constructor() {
      this.state = JSON.parse(JSON.stringify(INITIAL_STATE));
      this.activeProvinceId = 'thanjavur';
      this.listeners = new Set();
      this.hasStarted = false;
      this.load();
    }

    reset() {
      this.state = JSON.parse(JSON.stringify(INITIAL_STATE));
      this.activeProvinceId = 'thanjavur';
      this.save();
      this.notify();
    }

    save() {
      try {
        localStorage.setItem('YUG_CHOLA_STATE', JSON.stringify(this.state));
      } catch (e) {}
    }

    load() {
      try {
        const saved = localStorage.getItem('YUG_CHOLA_STATE');
        if (saved) {
          const parsed = JSON.parse(saved);
          if (parsed && typeof parsed.regnalYear === 'number') {
            this.state = parsed;
          }
        }
      } catch (e) {}
    }

    subscribe(fn) {
      this.listeners.add(fn);
      fn(this.state);
      return () => this.listeners.delete(fn);
    }

    notify() {
      for (const fn of this.listeners) {
        try { fn(this.state); } catch (e) { console.error(e); }
      }
    }

    setActiveProvince(id) {
      if (this.state.provinces[id]) {
        this.activeProvinceId = id;
        this.notify();
      }
    }

    canAfford(cost) {
      if (!cost) return true;
      for (const [res, amount] of Object.entries(cost)) {
        if ((this.state.resources[res] || 0) < amount) {
          return false;
        }
      }
      return true;
    }

    deductCost(cost) {
      if (!cost) return;
      for (const [res, amount] of Object.entries(cost)) {
        this.state.resources[res] = (this.state.resources[res] || 0) - amount;
      }
    }

    applyGains(gain) {
      if (!gain) return;
      for (const [k, v] of Object.entries(gain)) {
        if (this.state.resources[k] !== undefined) {
          this.state.resources[k] = Math.max(0, this.state.resources[k] + v);
        } else if (this.state.metrics[k] !== undefined) {
          this.state.metrics[k] = Math.min(100, Math.max(0, this.state.metrics[k] + v));
        }
      }
    }

    buildProject(provinceId, projectId) {
      const prov = this.state.provinces[provinceId];
      if (!prov) return { success: false, reason: 'Invalid province' };
      const proj = prov.projects.find(p => p.id === projectId);
      if (!proj) return { success: false, reason: 'Invalid project' };
      if (proj.completed) return { success: false, reason: 'Already completed' };
      if (proj.requires) {
        const req = prov.projects.find(p => p.id === proj.requires);
        if (req && !req.completed) {
          return { success: false, reason: `Requires prior completion of: ${req.title}` };
        }
      }
      if (!this.canAfford(proj.cost)) {
        return { success: false, reason: 'Insufficient imperial resources' };
      }

      this.deductCost(proj.cost);
      this.applyGains(proj.gain);
      proj.completed = true;
      prov.tier = Math.min(3, prov.tier + 1);

      if (window.score?.playSfx) window.score.playSfx('bell');
      this.save();
      this.notify();
      return { success: true };
    }

    getCurrentDecree() {
      if (this.state.yearIndex >= HISTORICAL_DECREES.length) return null;
      return HISTORICAL_DECREES[this.state.yearIndex];
    }

    resolveDecree(choiceIndex) {
      const decree = this.getCurrentDecree();
      if (!decree) return;
      const choice = decree.choices[choiceIndex];
      if (!choice) return;

      if (!this.canAfford(choice.cost)) {
        return { success: false, reason: 'Cannot afford this decree choice' };
      }

      this.deductCost(choice.cost);
      this.applyGains(choice.effect);

      this.state.decreesHistory.push({
        year: decree.year,
        title: decree.title,
        choiceLabel: choice.label,
        log: choice.log
      });

      // Seasonal yields & upkeep for the year
      const deltaYield = 300 + (this.state.provinces.gangaikonda.projects.some(p => p.id === 'gangaikonda_canals' && p.completed) ? 150 : 0);
      const goldYield = 180 + (this.state.provinces.nagapattinam.projects.some(p => p.id === 'nagapattinam_shipyard' && p.completed) ? 120 : 0);
      const stoneYield = 80;

      this.state.resources.grain += deltaYield;
      this.state.resources.gold += goldYield;
      this.state.resources.granite += stoneYield;

      // Upkeep
      this.state.resources.grain = Math.max(0, this.state.resources.grain - 180);
      this.state.resources.gold = Math.max(0, this.state.resources.gold - 80);

      this.state.yearIndex += 1;
      this.state.regnalYear += 1;
      this.state.calYear += (this.state.yearIndex < HISTORICAL_DECREES.length ? HISTORICAL_DECREES[this.state.yearIndex].calYear - decree.calYear : 1);

      if (window.score?.playSfx) window.score.playSfx('chime');
      this.save();
      this.notify();
      return { success: true, log: choice.log };
    }

    isGameOver() {
      return this.state.yearIndex >= HISTORICAL_DECREES.length;
    }

    getReignSummary() {
      const { grandeur, hydraulic, maritime, trust } = this.state.metrics;
      let imperialTitle = 'Rajakesarivarman Sri Rajendra Cholan';
      let titleTamil = 'ஸ்ரீ ராஜேந்திர சோழன்';
      let epithet = 'The Great Sovereign of the Three Worlds';

      if (maritime >= 75 && grandeur >= 60) {
        imperialTitle = 'Kadaram Kondan Sri Rajendra Chola';
        titleTamil = 'கடாரம் கொண்டான் ஸ்ரீ ராஜேந்திர சோழன்';
        epithet = 'The Emperor Who Conquered the Ocean Kingdoms & Malay Seas';
      } else if (hydraulic >= 70 && grandeur >= 70) {
        imperialTitle = 'Gangaikonda Cholan';
        titleTamil = 'கங்கை கொண்ட சோழன்';
        epithet = 'He Who Brought the Waters of Holy Ganga to the South';
      } else if (grandeur >= 80) {
        imperialTitle = 'Sivapadasekhara Deva';
        titleTamil = 'சிவபாதசேகர தேவன்';
        epithet = 'The Crown of Devotion and Architect of Immortal Stone';
      }

      const totalProjects = Object.values(this.state.provinces).reduce((sum, prov) => {
        return sum + prov.projects.filter(p => p.completed).length;
      }, 0);

      return {
        imperialTitle,
        titleTamil,
        epithet,
        totalProjects,
        finalMetrics: { ...this.state.metrics },
        finalResources: { ...this.state.resources },
        decreesHistory: this.state.decreesHistory
      };
    }
  }

  window.YUG_GAME = new CholaGameEngine();

  // --- UI Controller ---
  function initCholaGameUI() {
    const game = window.YUG_GAME;
    if (!game) return;

    // Elements
    const resGrain = document.getElementById('resGrain');
    const resGold = document.getElementById('resGold');
    const resGranite = document.getElementById('resGranite');
    const resFleet = document.getElementById('resFleet');

    const meterGrandeur = document.getElementById('meterGrandeur');
    const meterHydraulic = document.getElementById('meterHydraulic');
    const meterMaritime = document.getElementById('meterMaritime');
    const meterTrust = document.getElementById('meterTrust');

    const meterGrandeurVal = document.getElementById('meterGrandeurVal');
    const meterHydraulicVal = document.getElementById('meterHydraulicVal');
    const meterMaritimeVal = document.getElementById('meterMaritimeVal');
    const meterTrustVal = document.getElementById('meterTrustVal');

    const gameRegnalKicker = document.getElementById('gameRegnalKicker');
    const gameYearDisplay = document.getElementById('gameYearDisplay');

    const provTagline = document.getElementById('provTagline');
    const provName = document.getElementById('provName');
    const provTier = document.getElementById('provTier');
    const projectsList = document.getElementById('projectsList');

    const tabThanjavur = document.getElementById('tabThanjavur');
    const tabGangaikonda = document.getElementById('tabGangaikonda');
    const tabNagapattinam = document.getElementById('tabNagapattinam');

    const schematicThanjavur = document.getElementById('schematicThanjavur');
    const schematicGangaikonda = document.getElementById('schematicGangaikonda');
    const schematicNagapattinam = document.getElementById('schematicNagapattinam');

    const decreeRegnalYear = document.getElementById('decreeRegnalYear');
    const decreeTamil = document.getElementById('decreeTamil');
    const decreeCalYear = document.getElementById('decreeCalYear');
    const decreeTitle = document.getElementById('decreeTitle');
    const decreeContext = document.getElementById('decreeContext');
    const decreeOptions = document.getElementById('decreeOptions');
    const chronicleFeed = document.getElementById('chronicleFeed');

    const kudavolaiTriggerWrap = document.getElementById('kudavolaiTriggerWrap');
    const expeditionTriggerWrap = document.getElementById('expeditionTriggerWrap');
    const openKudavolaiBtn = document.getElementById('openKudavolaiBtn');
    const openExpeditionBtn = document.getElementById('openExpeditionBtn');

    const kudavolaiDialog = document.getElementById('kudavolaiDialog');
    const closeKudavolaiBtn = document.getElementById('closeKudavolaiBtn');
    const palmLeavesDeck = document.getElementById('palmLeavesDeck');
    const confirmKudavolaiBtn = document.getElementById('confirmKudavolaiBtn');

    const expeditionDialog = document.getElementById('expeditionDialog');
    const closeExpeditionBtn = document.getElementById('closeExpeditionBtn');
    const launchVoyageBtn = document.getElementById('launchVoyageBtn');
    const cargoSelect = document.getElementById('cargoSelect');
    const escortSelect = document.getElementById('escortSelect');

    const epilogueDialog = document.getElementById('epilogueDialog');
    const closeEpilogueBtn = document.getElementById('closeEpilogueBtn');
    const playAgainBtn = document.getElementById('playAgainBtn');
    const copyInscriptionBtn = document.getElementById('copyInscriptionBtn');
    const resetGameBtn = document.getElementById('resetGameBtn');

    // Subscribe to state updates
    game.subscribe(state => {
      // Update Resources
      if (resGrain) resGrain.textContent = state.resources.grain;
      if (resGold) resGold.textContent = state.resources.gold;
      if (resGranite) resGranite.textContent = state.resources.granite;
      if (resFleet) resFleet.textContent = state.resources.fleet;

      // Update Metrics
      const m = state.metrics;
      if (meterGrandeur) meterGrandeur.style.width = `${m.grandeur}%`;
      if (meterHydraulic) meterHydraulic.style.width = `${m.hydraulic}%`;
      if (meterMaritime) meterMaritime.style.width = `${m.maritime}%`;
      if (meterTrust) meterTrust.style.width = `${m.trust}%`;

      if (meterGrandeurVal) meterGrandeurVal.textContent = `${m.grandeur}%`;
      if (meterHydraulicVal) meterHydraulicVal.textContent = `${m.hydraulic}%`;
      if (meterMaritimeVal) meterMaritimeVal.textContent = `${m.maritime}%`;
      if (meterTrustVal) meterTrustVal.textContent = `${m.trust}%`;

      // Year badge
      if (gameRegnalKicker) gameRegnalKicker.textContent = `REGNAL YEAR ${String(state.regnalYear).padStart(2, '0')}`;
      if (gameYearDisplay) gameYearDisplay.textContent = `${state.calYear} CE · ${state.regnalYear <= 10 ? 'Golden Age' : 'Imperial Era'}`;

      // Update Province UI
      const activeProv = state.provinces[game.activeProvinceId];
      if (activeProv) {
        if (provTagline) provTagline.textContent = activeProv.tagline;
        if (provName) provName.textContent = activeProv.name;
        if (provTier) provTier.textContent = `Tier ${activeProv.tier}`;

        // Schematic SVGs
        if (schematicThanjavur) schematicThanjavur.hidden = game.activeProvinceId !== 'thanjavur';
        if (schematicGangaikonda) schematicGangaikonda.hidden = game.activeProvinceId !== 'gangaikonda';
        if (schematicNagapattinam) schematicNagapattinam.hidden = game.activeProvinceId !== 'nagapattinam';

        // Nav tabs
        [tabThanjavur, tabGangaikonda, tabNagapattinam].forEach(btn => {
          if (!btn) return;
          const isSelected = btn.dataset.prov === game.activeProvinceId;
          btn.setAttribute('aria-selected', String(isSelected));
          btn.classList.toggle('active', isSelected);
        });

        // Projects list
        if (projectsList) {
          projectsList.replaceChildren();
          activeProv.projects.forEach(p => {
            const card = document.createElement('div');
            card.className = `project-card ${p.completed ? 'completed' : ''}`;

            const top = document.createElement('div');
            top.className = 'project-card-top';

            const title = document.createElement('h5');
            title.textContent = p.title;

            const badge = document.createElement('span');
            badge.className = 'project-badge';
            badge.textContent = p.completed ? 'Completed ✓' : 'Available';
            top.append(title, badge);

            const desc = document.createElement('p');
            desc.className = 'project-desc';
            desc.textContent = p.desc;

            const meta = document.createElement('div');
            meta.className = 'project-meta';

            const costs = document.createElement('div');
            costs.className = 'project-costs';
            costs.innerHTML = '<span class="eyebrow">REQUIRED:</span> ' +
              Object.entries(p.cost).map(([k, v]) => `<span>${v} ${k}</span>`).join(' · ');

            const gains = document.createElement('div');
            gains.className = 'project-gains';
            gains.innerHTML = '<span class="eyebrow">YIELDS:</span> ' +
              Object.entries(p.gain).map(([k, v]) => `<span>+${v} ${k}</span>`).join(' · ');

            meta.append(costs, gains);

            const actionBtn = document.createElement('button');
            actionBtn.className = 'button primary project-build-btn';
            actionBtn.textContent = p.completed ? 'Monument Completed ✓' : 'Commission Construction';
            actionBtn.disabled = p.completed || !game.canAfford(p.cost);

            if (!p.completed) {
              actionBtn.addEventListener('click', () => {
                const res = game.buildProject(activeProv.id, p.id);
                if (res.success) {
                  addChronicleEntry(state.calYear, `Commissioned monumental project: ${p.title} in ${activeProv.name}.`);
                }
              });
            }

            card.append(top, desc, meta, actionBtn);
            projectsList.appendChild(card);
          });
        }
      }

      // Update Decree UI
      const decree = game.getCurrentDecree();
      if (decree) {
        if (decreeRegnalYear) decreeRegnalYear.textContent = decree.kicker;
        if (decreeTamil) decreeTamil.textContent = decree.tamil;
        if (decreeCalYear) decreeCalYear.textContent = `${decree.calYear} CE`;
        if (decreeTitle) decreeTitle.textContent = decree.title;
        if (decreeContext) decreeContext.textContent = decree.context;

        // Mini-game triggers
        if (kudavolaiTriggerWrap) kudavolaiTriggerWrap.hidden = decree.interactiveType !== 'kudavolai';
        if (expeditionTriggerWrap) expeditionTriggerWrap.hidden = decree.interactiveType !== 'expedition';

        // Decree choice buttons
        if (decreeOptions) {
          decreeOptions.replaceChildren();
          decree.choices.forEach((c, idx) => {
            const btn = document.createElement('button');
            btn.className = 'decree-choice-btn';

            const cTop = document.createElement('div');
            cTop.className = 'choice-header';

            const cLabel = document.createElement('strong');
            cLabel.textContent = c.label;

            const cCost = document.createElement('span');
            cCost.className = 'choice-cost';
            cCost.textContent = Object.keys(c.cost).length ?
              Object.entries(c.cost).map(([k, v]) => `Cost: ${v} ${k}`).join(' · ') : 'No Upfront Cost';

            cTop.append(cLabel, cCost);

            const cSum = document.createElement('p');
            cSum.className = 'choice-summary';
            cSum.textContent = c.summary;

            const cEffect = document.createElement('div');
            cEffect.className = 'choice-effect';
            cEffect.textContent = 'Predicted Impact: ' +
              Object.entries(c.effect).map(([k, v]) => `${v > 0 ? '+' : ''}${v} ${k}`).join(' · ');

            btn.append(cTop, cSum, cEffect);
            btn.disabled = !game.canAfford(c.cost);

            btn.addEventListener('click', () => {
              const res = game.resolveDecree(idx);
              if (res && res.success) {
                addChronicleEntry(decree.calYear, res.log);
                if (game.isGameOver()) {
                  showEpilogue();
                }
              }
            });

            decreeOptions.appendChild(btn);
          });
        }
      } else {
        // Game completed
        if (decreeTitle) decreeTitle.textContent = 'Reign of Rajendra Chola I Concluded';
        if (decreeContext) decreeContext.textContent = 'The decade of monumental stone, water, and naval dominance is inscribed upon the temple walls.';
        if (decreeOptions) {
          decreeOptions.replaceChildren();
          const viewBtn = document.createElement('button');
          viewBtn.className = 'button primary';
          viewBtn.textContent = 'View Perpetual Stone Inscription ↗';
          viewBtn.addEventListener('click', showEpilogue);
          decreeOptions.appendChild(viewBtn);
        }
      }
    });

    // Helper: Add entry to chronicle feed
    function addChronicleEntry(year, text) {
      if (!chronicleFeed) return;
      const entry = document.createElement('div');
      entry.className = 'chronicle-entry';
      entry.innerHTML = `<span class="chronicle-tag">${year} CE</span><p>${text}</p>`;
      chronicleFeed.prepend(entry);
    }

    // Province switching listeners
    if (tabThanjavur) tabThanjavur.addEventListener('click', () => game.setActiveProvince('thanjavur'));
    if (tabGangaikonda) tabGangaikonda.addEventListener('click', () => game.setActiveProvince('gangaikonda'));
    if (tabNagapattinam) tabNagapattinam.addEventListener('click', () => game.setActiveProvince('nagapattinam'));

    // Kudavolai ballot interaction
    let selectedLeaf = null;
    const candidates = [
      { name: 'Arulmozhi Varman', role: 'Eri-variyam (Tank Committee)', qual: 'Owns 1/2 veli land; memorized Rig & Sama hymns; clean accounts for 3 years; age 42.', valid: true },
      { name: 'Kandiyur Nathan', role: 'Samvatsara-variyam (Annual Council)', qual: 'Owns 1/4 veli land; built brick house on site; age 51; pious and incorruptible.', valid: true },
      { name: 'Paranthakan Velan', role: 'Totta-variyam (Garden Committee)', qual: 'Owns 1 veli land; age 27 (Disqualified: below minimum 35 years per inscription).', valid: false }
    ];

    function setupKudavolaiStage() {
      if (!palmLeavesDeck) return;
      palmLeavesDeck.replaceChildren();
      selectedLeaf = null;
      if (confirmKudavolaiBtn) confirmKudavolaiBtn.disabled = true;

      candidates.forEach((cand, idx) => {
        const leaf = document.createElement('div');
        leaf.className = 'palm-leaf-card';
        leaf.tabIndex = 0;
        leaf.innerHTML = `
          <div class="leaf-header">
            <span class="leaf-number">Lot #${idx + 1}</span>
            <strong>${cand.name}</strong>
          </div>
          <span class="leaf-role">${cand.role}</span>
          <p class="leaf-qual">${cand.qual}</p>
          <span class="leaf-status ${cand.valid ? 'valid' : 'invalid'}">${cand.valid ? 'Qualified Candidate ✓' : 'Disqualified under Uttiramerur Edict ✕'}</span>
        `;
        leaf.addEventListener('click', () => {
          palmLeavesDeck.querySelectorAll('.palm-leaf-card').forEach(c => c.classList.remove('selected'));
          leaf.classList.add('selected');
          selectedLeaf = cand;
          if (confirmKudavolaiBtn) confirmKudavolaiBtn.disabled = !cand.valid;
          if (window.score?.playSfx) window.score.playSfx('wood');
        });
        palmLeavesDeck.appendChild(leaf);
      });
    }

    if (openKudavolaiBtn) {
      openKudavolaiBtn.addEventListener('click', () => {
        setupKudavolaiStage();
        kudavolaiDialog.showModal();
      });
    }
    if (closeKudavolaiBtn) closeKudavolaiBtn.addEventListener('click', () => kudavolaiDialog.close());

    if (confirmKudavolaiBtn) {
      confirmKudavolaiBtn.addEventListener('click', () => {
        if (!selectedLeaf || !selectedLeaf.valid) return;
        kudavolaiDialog.close();
        game.resolveDecree(0);
        addChronicleEntry(1015, `The Kudavolai brass-pot ballot was drawn at Uttiramerur: ${selectedLeaf.name} confirmed for ${selectedLeaf.role}.`);
      });
    }

    // Maritime Expedition interaction
    if (openExpeditionBtn) {
      openExpeditionBtn.addEventListener('click', () => expeditionDialog.showModal());
    }
    if (closeExpeditionBtn) closeExpeditionBtn.addEventListener('click', () => expeditionDialog.close());

    if (launchVoyageBtn) {
      launchVoyageBtn.addEventListener('click', () => {
        expeditionDialog.close();
        const cargo = cargoSelect ? cargoSelect.value : 'pepper';
        const escorts = escortSelect ? parseInt(escortSelect.value, 10) : 2;
        game.resolveDecree(0);
        addChronicleEntry(1019, `Maritime Trade Armada sailed from Nagapattinam with ${escorts} warships carrying ${cargo.toUpperCase()}. The fleet docked at Srivijaya and Kedah with rich reciprocal gold and camphor.`);
      });
    }

    // Epilogue / Stone Inscription
    function showEpilogue() {
      const summary = game.getReignSummary();
      const epilogueTitle = document.getElementById('epilogueTitle');
      const epilogueEpithet = document.getElementById('epilogueEpithet');
      const epilogueProjects = document.getElementById('epilogueProjects');
      const epilogueGrandeur = document.getElementById('epilogueGrandeur');
      const epilogueHydraulic = document.getElementById('epilogueHydraulic');
      const epilogueMaritime = document.getElementById('epilogueMaritime');
      const epilogueText = document.getElementById('epilogueText');

      if (epilogueTitle) epilogueTitle.textContent = summary.imperialTitle;
      if (epilogueEpithet) epilogueEpithet.textContent = summary.epithet;
      if (epilogueProjects) epilogueProjects.textContent = `${summary.totalProjects} Completed`;
      if (epilogueGrandeur) epilogueGrandeur.textContent = `${summary.finalMetrics.grandeur} / 100`;
      if (epilogueHydraulic) epilogueHydraulic.textContent = `${summary.finalMetrics.hydraulic} / 100`;
      if (epilogueMaritime) epilogueMaritime.textContent = `${summary.finalMetrics.maritime} / 100`;

      if (epilogueText) {
        epilogueText.innerHTML = `
          <p>“In the reign of <strong>${summary.imperialTitle}</strong>, the sovereign of the Kaveri delta, whose war galleons furrowed the eastern ocean and whose granaries fed countless villages:</p>
          <p>Eight monumental works were raised in dressed granite. The waters of the Kaveri were gathered into the great sea-like reservoir of Cholaganga. Trade routes to Song China and Sumatra flourished under the imperial tiger standard, and the village assemblies cast their palm-leaf ballots in peace and justice.”</p>
          <p><em>Engraved upon eternal granite at Gangaikonda Cholapuram, 1025 CE.</em></p>
        `;
      }

      if (window.score?.playSfx) window.score.playSfx('bell');
      epilogueDialog.showModal();
    }

    if (closeEpilogueBtn) closeEpilogueBtn.addEventListener('click', () => epilogueDialog.close());

    if (playAgainBtn) {
      playAgainBtn.addEventListener('click', () => {
        epilogueDialog.close();
        game.reset();
        addChronicleEntry(1014, 'A new reign begins. The Chola throne awaits your wisdom.');
      });
    }

    if (copyInscriptionBtn) {
      copyInscriptionBtn.addEventListener('click', async () => {
        const text = document.getElementById('epilogueText')?.textContent || '';
        try {
          await navigator.clipboard.writeText(text);
          copyInscriptionBtn.textContent = 'Copied to Clipboard ✓';
          setTimeout(() => { copyInscriptionBtn.textContent = 'Copy Stone Inscription Text'; }, 2000);
        } catch (e) {}
      });
    }

    if (resetGameBtn) {
      resetGameBtn.addEventListener('click', () => {
        if (confirm('Start your Chola imperial reign anew? Current progress will reset.')) {
          game.reset();
          addChronicleEntry(1014, 'Imperial chronicle reset. Reign commenced anew at 1014 CE.');
        }
      });
    }

    // Initial render trigger
    game.notify();
  }

  window.initCholaGameUI = initCholaGameUI;
})();

