import torch
import torch.nn as nn
import torch.optim as optim
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import RobustScaler
import numpy as np
import pandas as pd
import optuna

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print(f"Usando dispositivo: {device}")

data = pd.read_csv('dataRain.csv')
assert not data.isnull().values.any(), "El dataset contiene valores nulos."


X = data[['UserIntensity']].values  
y = data[['Rainfall', 'WindGustSpeed', 'WindX', 'WindY', 'EmissionRate']].values 

X_train_full, X_test, y_train_full, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
X_train, X_val, y_train, y_val = train_test_split(X_train_full, y_train_full, test_size=0.2, random_state=42)


scaler_X = RobustScaler()
scaler_y = RobustScaler()

X_train_scaled = scaler_X.fit_transform(X_train)
X_val_scaled = scaler_X.transform(X_val)
X_test_scaled = scaler_X.transform(X_test)

y_train_scaled = scaler_y.fit_transform(y_train)
y_val_scaled = scaler_y.transform(y_val)
y_test_scaled = scaler_y.transform(y_test)

X_train_tensor = torch.tensor(X_train_scaled, dtype=torch.float32).to(device)
X_val_tensor = torch.tensor(X_val_scaled, dtype=torch.float32).to(device)
X_test_tensor = torch.tensor(X_test_scaled, dtype=torch.float32).to(device)

y_train_tensor = torch.tensor(y_train_scaled, dtype=torch.float32).to(device)
y_val_tensor = torch.tensor(y_val_scaled, dtype=torch.float32).to(device)
y_test_tensor = torch.tensor(y_test_scaled, dtype=torch.float32).to(device) 

class CVAE(nn.Module):
    def __init__(self, n_latent_dim=32, dropout_rate=0.3, hidden_dim=64):
        super(CVAE, self).__init__()

        # Encoder
        self.fc1 = nn.Linear(1, hidden_dim)
        self.fc2_mu = nn.Linear(hidden_dim, n_latent_dim)
        self.fc2_logvar = nn.Linear(hidden_dim, n_latent_dim)
        self.dropout = nn.Dropout(dropout_rate)

        # Decoder
        self.fc3 = nn.Linear(n_latent_dim + 1, hidden_dim)  
        self.fc4 = nn.Linear(hidden_dim, 128)
        self.fc5 = nn.Linear(128, 5) 

    def encoder(self, x):
        h1 = torch.relu(self.fc1(x))
        h1 = self.dropout(h1)
        mu = self.fc2_mu(h1)
        logvar = self.fc2_logvar(h1)
        return mu, logvar

    def reparametrize(self, mu, logvar):
        std = torch.exp(0.5 * logvar)
        eps = torch.randn_like(std)
        return mu + eps * std

    def decoder(self, z, c):
        z = torch.cat([z, c], dim=1)
        h3 = torch.relu(self.fc3(z))
        h3 = self.dropout(h3)
        h4 = torch.relu(self.fc4(h3))
        return self.fc5(h4)

    def forward(self, c):
    
        mu, logvar = self.encoder(c)
        z = self.reparametrize(mu, logvar)
        pred = self.decoder(z, c)
        return pred, mu, logvar

def loss_function(recon_x, x, mu, logvar):
    
    MSE = nn.MSELoss()(recon_x, x)
    KLD = -0.5 * torch.mean(1 + logvar - mu.pow(2) - logvar.exp())
    return MSE + 0.001 * KLD

def objective(trial):
    n_latent_dim = trial.suggest_int("n_latent_dim", 16, 64)
    dropout_rate = trial.suggest_float("dropout_rate", 0.1, 0.5)
    hidden_dim = trial.suggest_int("hidden_dim", 32, 128)
    lr = trial.suggest_float("lr", 1e-4, 1e-2, log=True)

    model = CVAE(n_latent_dim=n_latent_dim,
                 dropout_rate=dropout_rate,
                 hidden_dim=hidden_dim).to(device)
    optimizer = optim.Adam(model.parameters(), lr=lr)

    epochs = 50  
    for epoch in range(epochs):
        model.train()
        optimizer.zero_grad()
        y_pred, mu, logvar = model(X_train_tensor)
        loss = loss_function(y_pred, y_train_tensor, mu, logvar)
        loss.backward()
        optimizer.step()

    model.eval()
    with torch.no_grad():
        y_val_pred, mu_val, logvar_val = model(X_val_tensor)
        val_loss = loss_function(y_val_pred, y_val_tensor, mu_val, logvar_val).item()

    return val_loss

study = optuna.create_study(direction="minimize")
study.optimize(objective, n_trials=50)


def train_best_model(best_params):
    model = CVAE(n_latent_dim=best_params['n_latent_dim'],
                 dropout_rate=best_params['dropout_rate'],
                 hidden_dim=best_params['hidden_dim']).to(device)

    optimizer = optim.Adam(model.parameters(), lr=best_params['lr'])

    epochs = 200  
    for epoch in range(epochs):
        model.train()
        optimizer.zero_grad()
        y_pred, mu, logvar = model(X_train_tensor)
        loss = loss_function(y_pred, y_train_tensor, mu, logvar)
        loss.backward()
        optimizer.step()


        if (epoch + 1) % 50 == 0:
            print(f"Epoch [{epoch+1}/{epochs}], Loss: {loss.item():.4f}")

    return model

trained_model = train_best_model(study.best_params)


class CVAEInference(nn.Module):
    def __init__(self, trained_model, scaler_X, scaler_y):
        super(CVAEInference, self).__init__()

        self.trained_model = trained_model
        self.scaler_X_center = torch.tensor(scaler_X.center_, dtype=torch.float32).to(device)
        self.scaler_X_scale = torch.tensor(scaler_X.scale_, dtype=torch.float32).to(device)
        self.scaler_y_center = torch.tensor(scaler_y.center_, dtype=torch.float32).to(device)
        self.scaler_y_scale = torch.tensor(scaler_y.scale_, dtype=torch.float32).to(device)

    def forward(self, c_unscaled):
      
        c_scaled = (c_unscaled - self.scaler_X_center) / self.scaler_X_scale

       
        pred_scaled, _, _ = self.trained_model(c_scaled)

        pred_unscaled = pred_scaled * self.scaler_y_scale + self.scaler_y_center
        return pred_unscaled

inference_model = CVAEInference(trained_model, scaler_X, scaler_y)

example_input = torch.tensor([[0.0]], dtype=torch.float32).to(device)

torch.onnx.export(
    inference_model,
    example_input,
    "cvae_model_inference.onnx",
    input_names=["UserIntensity"],
    output_names=["Predictions"],
    dynamic_axes={'UserIntensity': {0: 'batch_size'},
                  'Predictions': {0: 'batch_size'}},
    opset_version=11
)


def test_with_user_intensity(model, intensity_values):
    model.eval()  

    for intensity in intensity_values:
        intensity_tensor = torch.tensor([[intensity]], dtype=torch.float32).to(device)

        with torch.no_grad():
            predictions = model(intensity_tensor)

        print(f"Resultados para UserIntensity = {intensity}: {predictions.cpu().numpy()}")


intensity_values = [0, 5, 10, 15]

test_with_user_intensity(inference_model, intensity_values)
