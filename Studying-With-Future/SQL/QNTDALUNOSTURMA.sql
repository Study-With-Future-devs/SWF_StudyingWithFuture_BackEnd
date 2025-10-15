SELECT 
    t.Id,
    t.Codigo,
    t.Descricao,
    COUNT(at.AlunoId) as QuantidadeAlunos
FROM Turmas t
LEFT JOIN AlunoTurmas at ON t.Id = at.TurmaId
GROUP BY t.Id, t.Codigo, t.Descricao;