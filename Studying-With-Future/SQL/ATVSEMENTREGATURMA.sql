SELECT 
    ativ.Id,
    ativ.Titulo,
    ativ.Descricao,
    ativ.DataEntrega
FROM Atividades ativ
LEFT JOIN Notas n ON ativ.Id = n.AtividadeId
WHERE ativ.TurmaId = [ID_DA_TURMA] AND n.AlunoId IS NULL;
