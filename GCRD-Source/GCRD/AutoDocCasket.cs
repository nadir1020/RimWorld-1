﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace GCRD
{
    public class AutoDocCasket : Building_CryptosleepCasket
    {
        private int _counter;
        private float _idlePowerConsumption;
        private float _maxPowerConsumption;
        private CompPowerTrader _powerTrader;
        private string _txtPowerConnectedRateStored = "Мощность сети/аккумулировано: {0} Вт / {1} Вт*дней";
        private string _txtPowerNeeded = "Потребляемая мощность";
        private string _txtPowerNotConnected = "Не подключено к сети.";
        private string _txtWatt = "Вт";

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            _txtWatt = "Watt".Translate();
            _txtPowerNotConnected = "PowerNotConnected".Translate();
            _txtPowerNeeded = "PowerNeeded".Translate();
            _txtPowerConnectedRateStored = "PowerConnectedRateStored".Translate();

            _powerTrader = GetComp<CompPowerTrader>();
            _maxPowerConsumption = _powerTrader.PowerOutput;
            _idlePowerConsumption = _maxPowerConsumption/10.0f;
        }

        public override void Tick()
        {
            base.Tick();

            if (!_powerTrader.PowerOn)
            {
                _powerTrader.PowerOutput = 0.0f;
                EjectContents();
                return;
            }

            if (container.Empty)
            {
                _powerTrader.PowerOutput = _idlePowerConsumption;
                return;
            }

            if (!container.Contents.Any()) return;

            ++_counter;
            if (_counter < 2500) return;
            _counter = 1;

            _powerTrader.PowerOutput = _maxPowerConsumption;

            var patients = new List<Pawn>();
            // Получаем содержимое крипто контейнера и заносим в список
            foreach (var pawn in container.Contents)
            {
                var patient = pawn as Pawn;
                if (patient != null) patients.Add(patient);
            }

            // Работаем с каждым пациентом отдельно
            foreach (var patient in patients)
            {
                patient.
                var healthTracker = patient.healthTracker;

                // Если здоров, выпускаем из капсулы
                if (!healthTracker.NeedsTreatment)
                {
                    EjectContents();
                    return;
                }

                foreach (var injury in
                    from x in healthTracker.bodyModel.GetTreatableInjuries()
                    orderby x.damageAmount descending
                    select x)
                {
                    
                    injury.isTreated = true;
                    injury.treatmentQuality = 1.0f;
                    injury.treatedWithMedicine = true;
                }

                foreach (var immunity in healthTracker.bodyModel.immunities)
                {
                    immunity.immunity = 1.0f;
                }
            }

            EjectContents();
            _counter = 0;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos();
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();

            if (_powerTrader == null) return stringBuilder.ToString();

            if (_powerTrader.PowerNet.hasPowerSource)
            {
                var pcrs = (int)_powerTrader.PowerNet.CurrentEnergyGainRate()/CompPower.WattsToWattDaysPerTick;
                stringBuilder.AppendLine(_txtPowerNeeded + ": " + -_powerTrader.PowerOutput + " " + _txtWatt);
                stringBuilder.AppendLine(string.Format(_txtPowerConnectedRateStored, pcrs,
                    _powerTrader.PowerNet.CurrentStoredEnergy().ToString("F0")));
            }
            else
            {
                stringBuilder.AppendLine(_txtPowerNotConnected);
            }

            return stringBuilder.ToString();
        }
    }
}