using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class ARNetworkSelectionInteractable : ARSelectionInteractable
{

    private PhoneRepresentation _phoneRep;
    private bool _isProcessingSelection = false;

    // Stato interno per gestire la selezione
    private bool _isNetworkSelectionAllowed = false;

    protected override void Awake()
    {
        base.Awake();

        _phoneRep = FindObjectOfType<PhoneRepresentation>();

        // Disabilita la selezione automatica di XRI
        // Vogliamo controllare manualmente quando la selezione è valida
        selectMode = InteractableSelectMode.Multiple;
    }

    // Metodo per forzare la deselezione (chiamato da PhoneRepresentation)
    /*public void ForceDeselect()
    {
        // Annulla la selezione XRI se è attiva
        if (isSelected)
        {
            interactionManager.SelectExit(interactorSelecting, this);
        }
    }*/

    // Metodo chiamato quando la selezione XRI inizia
    protected override async void OnSelectEntering(SelectEnterEventArgs args)
    {
        if (_isProcessingSelection) return; // Evita chiamate multiple
        _isProcessingSelection = true;

        // 1. Prova a selezionare l'oggetto tramite PhoneRepresentation
        bool selectionSuccess = await _phoneRep.TrySelectObject(GetComponent<MovableObject>());
        Debug.LogWarning("OnS.En.ing: " + this.name);

        if (!selectionSuccess)
        {
            // Se la selezione fallisce, annulla l'evento XRI
            interactionManager.SelectExit(args.interactorObject, this);
            _isProcessingSelection = false;
            return;
        }

        // 2. Se la selezione ha successo, procedi con la logica XRI standard
        base.OnSelectEntering(args);
        _isProcessingSelection = false;
    }

    // Metodo chiamato quando la selezione XRI termina
    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        // 1. Notifica PhoneRepresentation della deselezione
        _phoneRep.ResetSelectedObjectRPC();

        // 2. Esegui la logica XRI standard
        base.OnSelectExiting(args);
    }
}
