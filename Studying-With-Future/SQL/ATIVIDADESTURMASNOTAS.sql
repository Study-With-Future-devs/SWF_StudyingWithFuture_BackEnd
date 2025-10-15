SELECT 
    ativ.Id as AtividadeId,
    ativ.Titulo,
    ativ.Descricao,
    ativ.DataEntrega,
    a.Nome as Aluno,
    n.Valor as Nota,
    n.Observacao
FROM Atividades ativ
LEFT JOIN Notas n ON ativ.Id = n.AtividadeId
LEFT JOIN Alunos a ON n.AlunoId = a.Id
WHERE ativ.TurmaId = [ID_DA_TURMA];