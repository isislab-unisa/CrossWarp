using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisGizmoDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private void Start()
    {
        // Aggiungi un componente LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        // Configurazioni del LineRenderer
        lineRenderer.positionCount = 6; // 2 punti per ogni asse (X, Y, Z)
        lineRenderer.startWidth = 0.02f; // Spessore delle linee
        lineRenderer.endWidth = 0.02f;

        // Assegna colori agli assi
        lineRenderer.startColor = Color.red; // Colore dell'asse X
        lineRenderer.endColor = Color.red;

        // Imposta le posizioni iniziali
        UpdateAxisPositions();
    }

    private void Update()
    {
        // Aggiorna le posizioni se necessario (ad esempio, se l'oggetto si muove)
        UpdateAxisPositions();
    }

    private void UpdateAxisPositions()
    {
        Vector3 position = transform.position;

        // Asse X
        lineRenderer.SetPosition(0, position); // Inizio asse X
        lineRenderer.SetPosition(1, position + Vector3.right); // Fine asse X

        // Colore asse Y
        lineRenderer.startColor = Color.green; // Cambia colore per l'asse Y
        lineRenderer.SetPosition(2, position); // Inizio asse Y
        lineRenderer.SetPosition(3, position + Vector3.up); // Fine asse Y

        // Colore asse Z
        lineRenderer.startColor = Color.blue; // Cambia colore per l'asse Z
        lineRenderer.SetPosition(4, position); // Inizio asse Z
        lineRenderer.SetPosition(5, position + Vector3.forward); // Fine asse Z

        // Ripristina il colore iniziale per la linea successiva
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }
}
